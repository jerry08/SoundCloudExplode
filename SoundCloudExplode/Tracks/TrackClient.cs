﻿using System;
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
/// <remarks>
/// Initializes an instance of <see cref="TrackClient"/>.
/// </remarks>
public class TrackClient(HttpClient http, SoundcloudEndpoint endpoint)
{
    private readonly Regex ShortUrlRegex = new(@"on\.soundcloud\..+?\/.+?");
    private readonly Regex SingleTrackRegex = new(
        @"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+"
    );
    private readonly Regex TracksRegex = new(@"soundcloud\..+?\/(.*?)\/track");

    /// <summary>
    /// Checks for valid track(s) url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
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

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
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

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
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

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async ValueTask<Track?> GetByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default
    )
    {
        var trackUrl = await GetUrlByIdAsync(trackId, cancellationToken);
        return trackUrl is null ? null : await GetAsync(trackUrl, cancellationToken);
    }

    private async ValueTask<string?> QueryTrackMp3Async(
        string trackM3u8,
        CancellationToken cancellationToken = default
    )
    {
        var html = await http.ExecuteGetAsync(trackM3u8, cancellationToken);
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
        CancellationToken cancellationToken = default
    )
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
        CancellationToken cancellationToken = default
    )
    {
        if (track.Policy?.ToLower() == "block")
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

        var transcoding =
            // Progrssive/Stream - high quality
            track.Media.Transcodings.FirstOrDefault(x =>
                x.Quality == "hq"
                && x.Format?.MimeType?.Contains("audio/mp4") == true
                && x.Format.Protocol == "progressive"
            )
            ?? // Hls - high quality
            track.Media.Transcodings.FirstOrDefault(x =>
                x.Quality == "hq"
                && x.Format?.MimeType?.Contains("audio/mp4") == true
                && x.Format.Protocol == "hls"
            )
            ?? // Progrssive/Stream - standard quality
            track.Media.Transcodings.FirstOrDefault(x =>
                x.Quality == "sq"
                && x.Format?.MimeType?.Contains("audio/mpeg") == true
                && x.Format.Protocol == "progressive"
            )
            ?? // Hls  - standard quality
            track.Media.Transcodings.FirstOrDefault(x =>
                x.Quality == "sq"
                && x.Format?.MimeType?.Contains("ogg") == true
                && x.Format.Protocol == "hls"
            );

        if (transcoding is null || transcoding.Url is null)
            return null;

        var trackUrl = transcoding.Url.ToString() + $"?client_id={endpoint.ClientId}";

        var trackMedia = await http.ExecuteGetAsync(trackUrl, cancellationToken);

        var trackMediaUrl = JsonDocument
            .Parse(trackMedia)
            .RootElement.GetProperty("url")
            .GetString();

        return trackMediaUrl?.Contains(".m3u8") == true
            ? await QueryTrackMp3Async(trackMediaUrl, cancellationToken)
            : trackMediaUrl;
    }
}
