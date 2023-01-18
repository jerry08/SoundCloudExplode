using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Track;

/// <summary>
/// Operations related to Soundcloud tracks.
/// </summary>
public class TrackClient
{
    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

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
    /// Checks for valid track(s) url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public bool IsUrlValid(string url)
    {
        url = url.ToLower();
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        return isUrl && (TracksRegex.IsMatch(url) || SingleTrackRegex.IsMatch(url));
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    public async ValueTask<TrackInformation?> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid track url");

        var resolvedJson = await _endpoint.ResolveUrlAsync(url, cancellationToken);

        return JsonConvert.DeserializeObject<TrackInformation>(resolvedJson)!;
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    public async ValueTask<string?> GetUrlByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var trackInfoJson = await _http.ExecuteGetAsync($"{Constants.TrackEndpoint}/{trackId}?client_id={_endpoint.ClientId}", cancellationToken);
        if (trackInfoJson is null)
            return null;

        var track = JsonConvert.DeserializeObject<TrackInformation>(trackInfoJson);
        return track is null || track.PermalinkUrl is null ? null : (track.PermalinkUrl?.ToString());
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    public async ValueTask<TrackInformation?> GetByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var trackUrl = await GetUrlByIdAsync(trackId, cancellationToken);
        return trackUrl is null ? null : await GetAsync(trackUrl, cancellationToken);
    }

    /// <summary>
    /// Enumerates batches of tracks included in the specified url.
    /// </summary>
    public async IAsyncEnumerable<Batch<TrackInformation>> GetTrackBatchesAsync(
        string url,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid track url");

        var resolvedJson = await _endpoint.ResolveUrlAsync(url, cancellationToken);

        if (!TracksRegex.IsMatch(url))
        {
            var tracks = new List<TrackInformation>
            {
                JsonConvert.DeserializeObject<TrackInformation>(resolvedJson)!
            };

            yield return Batch.Create(tracks);
            yield break;
        }

        var user = JsonConvert.DeserializeObject<User.User>(resolvedJson);

        var next_href = default(string?);

        while (true)
        {
            var tracks = new List<TrackInformation>();

            if (user is null)
            {
                break;
            }

            //url = $"https://api-v2.soundcloud.com/users/{user.Id}/tracks?offset=2014-08-15T00&limit=20&representation=&client_id=eLWKwhCY4BrKxcpiyhjyr6SeHiszzUq6&app_locale=en";
            url = next_href ?? $"https://api-v2.soundcloud.com/users/{user.Id}/tracks?limit=200&client_id={_endpoint.ClientId}";

            var tracksJson = await _http.ExecuteGetAsync(url, cancellationToken);
            var tracksJObj = JObject.Parse(tracksJson);
            var collToken = tracksJObj?["collection"]?.ToString();

            if (collToken is not null && JsonConvert.DeserializeObject
                <List<TrackInformation>>(collToken) is List<TrackInformation> list)
            {
                tracks.AddRange(list);
                yield return Batch.Create(tracks);
            }

            if (!tracks.Any())
                break;

            next_href = tracksJObj?["next_href"]?.ToString();

            if (string.IsNullOrEmpty(next_href))
                break;

            next_href += $"&client_id={_endpoint.ClientId}";
        }
    }

    /// <summary>
    /// Enumerates tracks included in the specified url.
    /// </summary>
    public IAsyncEnumerable<TrackInformation> GetTracksAsync(
        string url,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, cancellationToken).FlattenAsync();

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
    /// Gets the download url from a track
    /// </summary>
    public async ValueTask<string?> GetDownloadUrlAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        var track = await GetAsync(url, cancellationToken);
        if (track is null)
            return null;

        return await GetDownloadUrlAsync(track);
    }

    /// <summary>
    /// Gets the download url from a track
    /// </summary>
    public async ValueTask<string?> GetDownloadUrlAsync(
        TrackInformation track,
        CancellationToken cancellationToken = default)
    {
        if (track.Policy?.ToLower() == "block")
        {
            throw new TrackUnavailableException("This track is not available in your country");
        }

        if (track.Media is null
            || track.Media.Transcodings is null
            || track.Media.Transcodings.Length == 0)
        {
            throw new TrackUnavailableException("No transcodings found");
        }

        var trackUrl = "";

        //progrssive/stream
        var transcoding = track.Media.Transcodings.FirstOrDefault(x => x.Quality == "sq"
            && x.Format?.MimeType?.Contains("audio/mpeg") == true && x.Format.Protocol == "progressive");

        //hls
        transcoding ??= track.Media.Transcodings.FirstOrDefault(x => x.Quality == "sq"
            && x.Format?.MimeType?.Contains("ogg") == true && x.Format.Protocol == "hls");

        if (transcoding is null || transcoding.Url is null)
            return null;

        trackUrl += transcoding.Url.ToString() + $"?client_id={_endpoint.ClientId}";

        var trackMedia = await _http.ExecuteGetAsync(trackUrl, cancellationToken);

        var track2 = JsonConvert.DeserializeObject<TrackMediaInformation>(trackMedia);
        if (track2 is null)
            return null;

        var trackMediaUrl = track2.Url ?? "";
        return trackMediaUrl.Contains(".m3u8") ? await QueryTrackMp3Async(trackMediaUrl, cancellationToken) : trackMediaUrl;
    }
}