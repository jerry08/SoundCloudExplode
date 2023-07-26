using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using SoundCloudExplode.Bridge;
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
public class SoundCloudClient
{
    internal string ClientId { get; private set; }

    //private readonly Regex PlaylistRegex = new (@"soundcloud\..+?/(.*?)\/sets\/.+");
    //private readonly Regex TrackRegex = new (@"soundcloud\..+?/(.*?)\/sets\/.+");
    private readonly Regex PlaylistRegex = new(@"soundcloud\..+?\/(.*?)\/sets\/[a-zA-Z]+");
    private readonly Regex TrackRegex = new(@"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+");

    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    private readonly string BaseUrl = "https://soundcloud.com";

    /// <summary>
    /// Operations related to Soundcloud search.
    /// </summary>
    public SearchClient Search { get; }

    /// <summary>
    /// Operations related to Soundcloud tracks.
    /// </summary>
    public TrackClient Tracks { get; }

    /// <summary>
    /// Operations related to Soundcloud playlists/albums.
    /// </summary>
    public PlaylistClient Playlists { get; }

    /// <summary>
    /// Operations related to Soundcloud users.
    /// </summary>
    public UserClient Users { get; }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(string clientId, HttpClient http)
    {
        ClientId = clientId;
        _http = http;

        _endpoint = new(http)
        {
            ClientId = clientId
        };

        Search = new(http, _endpoint);
        Tracks = new(http, _endpoint);
        Playlists = new(http, _endpoint);
        Users = new(http, _endpoint);
    }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient() : this(Constants.ClientId, Http.Client)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(HttpClient http) : this(Constants.ClientId, http)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(string clientId) : this(Http.Client)
    {
        ClientId = clientId;
    }

    /// <summary>
    /// Sets Default ClientId
    /// </summary>
    public async Task SetClientIdAsync(CancellationToken cancellationToken = default)
    {
        ClientId = await GetClientIdAsync(cancellationToken);
        _endpoint.ClientId = ClientId;
    }

    /// <summary>
    /// Gets ClientId
    /// </summary>
    public async Task<string> GetClientIdAsync(CancellationToken cancellationToken = default)
    {
        var response = await _http.ExecuteGetAsync(BaseUrl, cancellationToken);
        var document = new HtmlDocument();
        document.LoadHtml(response);

        var script = document.DocumentNode.Descendants()
            .Where(x => x.Name == "script").ToList();

        var script_url = script.Last().Attributes["src"].Value;

        response = await _http.ExecuteGetAsync(script_url, cancellationToken);

        return response.Split(new string[] { ",client_id" }, StringSplitOptions.None)[1].Split('"')[1];
    }

    /// <summary>
    /// Downloads a track
    /// </summary>
    public async ValueTask DownloadAsync(
        Track track,
        string filePath,
        IProgress<double>? progress = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default)
    {
        var mp3TrackMediaUrl = await Tracks.GetDownloadUrlAsync(track, cancellationToken);
        if (mp3TrackMediaUrl is null)
            return;

        var dir = Path.GetDirectoryName(filePath);
        if (dir is null)
            return;

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        using var destination = File.Create(filePath);

        var request = new HttpRequestMessage(HttpMethod.Get, mp3TrackMediaUrl);

        if (headers is not null)
        {
            foreach (var (key, value) in headers)
                request.Headers.TryAddWithoutValidation(key, value);
        }

        var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode})." +
                Environment.NewLine +
                "Request:" +
                Environment.NewLine +
                request
            );
        }

        var totalLength = response.Content.Headers.ContentLength ?? 0;

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);

        await stream.CopyToAsync(
            destination,
            totalLength,
            progress,
            cancellationToken: cancellationToken);
    }
}