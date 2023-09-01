using System;
using System.Linq;
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
public class TrackClient
{
    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    private readonly Regex ShortUrlRegex = new(@"on\.soundcloud\..+?\/.+?");
    private readonly Regex SingleTrackRegex = new(@"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+");
    private readonly Regex TracksRegex = new(@"soundcloud\..+?\/(.*?)\/track");

    /// <summary>
    /// Initializes an instance of <see cref="TrackClient"/>.
    /// </summary>
    public TrackClient(HttpClient http, SoundcloudEndpoint endpoint)
    {
        _http = http;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Checks for valid track(s) url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async Task<bool> IsUrlValidAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (ShortUrlRegex.IsMatch(url))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            url = response.RequestMessage!.RequestUri!.ToString();
        }

        url = url.ToLower();
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        return isUrl && (TracksRegex.IsMatch(url) || SingleTrackRegex.IsMatch(url));
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async ValueTask<Track?> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!await IsUrlValidAsync(url, cancellationToken))
            throw new SoundcloudExplodeException("Invalid track url");

        var resolvedJson = await _endpoint.ResolveUrlAsync(url, cancellationToken);

        return JsonSerializer.Deserialize<Track>(resolvedJson)!;
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async ValueTask<string?> GetUrlByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var trackInfoJson = await _http.ExecuteGetAsync($"{Constants.TrackEndpoint}/{trackId}?client_id={_endpoint.ClientId}", cancellationToken);
        if (trackInfoJson is null)
            return null;

        var track = JsonSerializer.Deserialize<Track>(trackInfoJson);
        return track is null || track.PermalinkUrl is null ? null : (track.PermalinkUrl?.ToString());
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async ValueTask<Track?> GetByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var trackUrl = await GetUrlByIdAsync(trackId, cancellationToken);
        return trackUrl is null ? null : await GetAsync(trackUrl, cancellationToken);
    }

    private async ValueTask<string?> QueryTrackMp3Async(
        string trackM3u8,
        CancellationToken cancellationToken = default)
    {
        var html = await _http.ExecuteGetAsync(trackM3u8, cancellationToken);
        var m3u8 = html.Split(',');

        if (m3u8.Length == 0)
            return null;

        var link = "";

        var last_stream = m3u8.Last().Split('/');
        for (int i = 0; i < last_stream.Length; i++)
        {
            if (last_stream[i] == "media")
            {
                last_stream[i + 1] = "0";
                link = string.Join("/", last_stream).Split('\n')[1];
            }
        }

        return link;
    }

    /// <summary>
    /// Gets the download url from a track's url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async ValueTask<string?> GetDownloadUrlAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var track = await GetAsync(url, cancellationToken);
        if (track is null)
            return null;

        return await GetDownloadUrlAsync(track, cancellationToken);
    }

    /// <summary>
    /// Gets the download url from a track.
    /// </summary>
    /// <exception cref="TrackUnavailableException"/>
    public async ValueTask<string?> GetDownloadUrlAsync(
        Track track,
        CancellationToken cancellationToken = default)
    {
        if (track.Policy?.ToLower() == "block")
        {
            throw new TrackUnavailableException("This track is not available in your country");
        }

        if (track.Media is null
            || track.Media.Transcodings is null
            || track.Media.Transcodings.Count == 0)
        {
            throw new TrackUnavailableException("No transcodings found");
        }

        var trackUrl = "";

        // Progrssive/Stream
        var transcoding = track.Media.Transcodings.FirstOrDefault(x => x.Quality == "sq"
            && x.Format?.MimeType?.Contains("audio/mpeg") == true && x.Format.Protocol == "progressive");

        // Hls
        transcoding ??= track.Media.Transcodings.FirstOrDefault(x => x.Quality == "sq"
            && x.Format?.MimeType?.Contains("ogg") == true && x.Format.Protocol == "hls");

        if (transcoding is null || transcoding.Url is null)
            return null;

        trackUrl += transcoding.Url.ToString() + $"?client_id={_endpoint.ClientId}";

        var trackMedia = await _http.ExecuteGetAsync(trackUrl, cancellationToken);

        var trackMediaUrl = JsonDocument.Parse(trackMedia)
            .RootElement.GetProperty("url").GetString();

        return trackMediaUrl?.Contains(".m3u8") == true
            ? await QueryTrackMp3Async(trackMediaUrl, cancellationToken)
            : trackMediaUrl;
    }
}