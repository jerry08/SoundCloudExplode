using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Tracks;

/// <summary>
/// Operations related to Soundcloud tracks.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="TrackClient"/>.
/// </remarks>
public class TrackClient(HttpClient http, SoundcloudEndpoint endpoint) : ITrackClient
{
    private readonly Regex ShortUrlRegex = new(@"on\.soundcloud\..+?\/.+?");
    private readonly Regex SingleTrackRegex = new(
        @"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+"
    );
    private readonly Regex TracksRegex = new(@"soundcloud\..+?\/(.*?)\/track");

    /// <inheritdoc />
    public async Task<bool> IsUrlValidAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            return false;

        url = url.ToLower();

        if (ShortUrlRegex.IsMatch(url))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            url = response.RequestMessage!.RequestUri!.ToString();
        }

        return TracksRegex.IsMatch(url) || SingleTrackRegex.IsMatch(url);
    }

    /// <inheritdoc />
    public async ValueTask<Track?> GetAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        if (!await IsUrlValidAsync(url, cancellationToken))
            throw new SoundcloudExplodeException("Invalid track url");

        var resolvedJson = await endpoint.ResolveUrlAsync(url, cancellationToken);

        return JsonSerializer.Deserialize(resolvedJson, SourceGenerationContext.Default.Track)!;
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetUrlByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default
    )
    {
        var trackInfoJson = await http.ExecuteGetAsync(
            $"{Constants.TrackEndpoint}/{trackId}?client_id={endpoint.ClientId}",
            cancellationToken
        );
        if (trackInfoJson is null)
            return null;

        var track = JsonSerializer.Deserialize(
            trackInfoJson,
            SourceGenerationContext.Default.Track
        );
        return track is null || track.PermalinkUrl is null
            ? null
            : (track.PermalinkUrl?.ToString());
    }

    /// <inheritdoc />
    public async ValueTask<Track?> GetByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default
    )
    {
        var trackUrl = await GetUrlByIdAsync(trackId, cancellationToken);
        return trackUrl is null ? null : await GetAsync(trackUrl, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetDownloadUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var track = await GetAsync(url, cancellationToken);
        if (track is null)
            return null;

        return await GetDownloadUrlAsync(track, cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask<string?> GetDownloadUrlAsync(
        Track track,
        CancellationToken cancellationToken = default
    )
    {
        if (string.Equals(track.Policy, "block", StringComparison.OrdinalIgnoreCase))
        {
            throw new TrackUnavailableException("This track is not available in your country");
        }

        if (
            track.Media is null
            || track.Media.Transcodings is null
            || track.Media.Transcodings.Count == 0
        )
        {
            throw new TrackUnavailableException("No transcodings found");
        }

        var transcoding = track.GetPreferredTranscoding();

        if (transcoding is null || transcoding.Url is null)
            return null;

        var builder = new UriBuilder(transcoding.Url);
        var query = builder.Query.TrimStart('?');
        builder.Query =
            (query.Length > 0 ? query + "&" : "")
            + $"client_id={Uri.EscapeDataString(endpoint.ClientId)}";

        var trackMedia = await http.ExecuteGetAsync(builder.Uri.AbsoluteUri, cancellationToken);

        using var document = JsonDocument.Parse(trackMedia);
        var trackMediaUrl = document.RootElement.GetProperty("url").GetString();

        // HLS URLs must remain playlists. Rewriting a segment URL can truncate the track
        // and no longer works for SoundCloud's fragmented MP4 streams.
        return string.IsNullOrWhiteSpace(trackMediaUrl) ? null : trackMediaUrl;
    }
}
