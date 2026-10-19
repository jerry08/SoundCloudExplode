using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Playlists;
using SoundCloudExplode.Search;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Users;
using SoundCloudExplode.Utils;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode;

/// <summary>
/// Initializes an instance of <see cref="SoundCloudClient"/>.
/// </summary>
public class SoundCloudClient : ISoundCloudClient
{
    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    private readonly string BaseUrl = "https://soundcloud.com";

    public string ClientId { get; set; }

    public bool IsInitialized { get; private set; }

    /// <summary>
    /// Operations related to Soundcloud search.
    /// </summary>
    public ISearchClient Search { get; }

    /// <summary>
    /// Operations related to Soundcloud tracks.
    /// </summary>
    public ITrackClient Tracks { get; }

    /// <summary>
    /// Operations related to Soundcloud playlists/albums.
    /// </summary>
    public IPlaylistClient Playlists { get; }

    /// <summary>
    /// Operations related to Soundcloud users.
    /// </summary>
    public IUserClient Users { get; }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(string clientId, HttpClient http, string? authorization = null)
    {
        ClientId = clientId;
        _http = http;

        if (!string.IsNullOrWhiteSpace(authorization))
            http.DefaultRequestHeaders.Add("Authorization", authorization);

        _endpoint = new(http) { ClientId = clientId };

        Search = new SearchClient(http, _endpoint);
        Tracks = new TrackClient(http, _endpoint);
        Playlists = new PlaylistClient(http, _endpoint);
        Users = new UserClient(http, _endpoint);
    }

    /// <inheritdoc cref="SoundCloudClient"/>
    public SoundCloudClient(string clientId, HttpClient http)
        : this(clientId, http, null) { }

    /// <inheritdoc cref="SoundCloudClient"/>
    public SoundCloudClient()
        : this(Constants.ClientId, Http.Client) { }

    /// <inheritdoc cref="SoundCloudClient"/>
    public SoundCloudClient(HttpClient http)
        : this(Constants.ClientId, http) { }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(string clientId, string authorization)
        : this(clientId, Http.Client, authorization) { }

    /// <inheritdoc cref="SoundCloudClient"/>
    public SoundCloudClient(string clientId)
        : this(clientId, Http.Client) { }

    /// <summary>
    /// Sets Default ClientId
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ClientId = await GetClientIdAsync(cancellationToken);
        _endpoint.ClientId = ClientId;

        IsInitialized = true;
    }

    /// <summary>
    /// Gets ClientId
    /// </summary>
    public async Task<string> GetClientIdAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.ExecuteGetAsync(BaseUrl, cancellationToken);

        var scripts = Regex.Matches(response, "<script.*?src=\"(.*?)\"");

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
        var scriptUrl = scripts.LastOrDefault()?.Groups[1].Value;
#else
        var scriptUrl = scripts.Cast<Match>().LastOrDefault()?.Groups[1].Value;
#endif

        if (string.IsNullOrEmpty(scriptUrl))
            return string.Empty;

        response = await _http.ExecuteGetAsync(scriptUrl, cancellationToken);

        return response.Split([",client_id"], StringSplitOptions.None)[1].Split('"')[1];
    }

    /// <summary>
    /// Gets url kind of a SoundCloud link
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Url <see cref="Kind"/></returns>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public async Task<Kind> GetUrlKindAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var resolvedJson = await _endpoint.ResolveUrlAsync(url, cancellationToken);
        if (string.IsNullOrWhiteSpace(resolvedJson))
            throw new SoundcloudExplodeException("Failed to resolve url.");

        return JsonSerializer.Deserialize(
            JsonNode.Parse(resolvedJson)!["kind"],
            SourceGenerationContext.Default.Kind
        );
    }

    /// <summary>
    /// Downloads progressive audio or assembles an unencrypted, on-demand HLS stream.
    /// The source audio encoding is preserved.
    /// </summary>
    public async ValueTask DownloadAsync(
        Track track,
        string filePath,
        IProgress<double>? progress = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    )
    {
        var mediaUrl =
            await Tracks.GetDownloadUrlAsync(track, cancellationToken)
            ?? throw new TrackUnavailableException("No supported audio stream found.");
        using var request = CreateDownloadRequest(new Uri(mediaUrl), headers);
        using var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var responseUrl = response.RequestMessage?.RequestUri ?? request.RequestUri!;
        IReadOnlyList<Uri>? segments = null;
        if (
            track.GetPreferredTranscoding()?.Format?.Protocol == "hls"
            || HlsPlaylist.IsPlaylist(responseUrl, response.Content)
        )
        {
            segments = await HlsPlaylist.GetSegmentsAsync(
                responseUrl,
                await response.Content.ReadAsStringAsync(cancellationToken),
                async url =>
                {
                    using var playlistRequest = CreateDownloadRequest(url, headers);
                    using var playlistResponse = await _http.SendAsync(
                        playlistRequest,
                        cancellationToken
                    );
                    playlistResponse.EnsureSuccessStatusCode();
                    return await playlistResponse.Content.ReadAsStringAsync(cancellationToken);
                }
            );
        }

        var fullPath = Path.GetFullPath(filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        using var destination = File.Create(fullPath);

        if (segments is null)
        {
            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await stream.CopyToAsync(
                destination,
                response.Content.Headers.ContentLength ?? 0,
                progress,
                cancellationToken: cancellationToken
            );
        }
        else
        {
            for (var i = 0; i < segments.Count; i++)
            {
                using var segmentRequest = CreateDownloadRequest(segments[i], headers);
                using var segmentResponse = await _http.SendAsync(
                    segmentRequest,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken
                );
                segmentResponse.EnsureSuccessStatusCode();
                using var stream = await segmentResponse.Content.ReadAsStreamAsync(
                    cancellationToken
                );
                await stream.CopyToAsync(destination, 81920, cancellationToken);
                progress?.Report((i + 1.0) / segments.Count);
            }
        }
    }

    private static HttpRequestMessage CreateDownloadRequest(
        Uri url,
        Dictionary<string, string>? headers
    )
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (headers is not null)
        {
            foreach (var (key, value) in headers)
                request.Headers.TryAddWithoutValidation(key, value);
        }

        return request;
    }
}
