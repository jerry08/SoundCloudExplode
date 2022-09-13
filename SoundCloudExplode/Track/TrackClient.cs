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
using SoundCloudExplode.Http;
using SoundCloudExplode.Track;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Tracks;

/// <summary>
/// Operations related to Soundcloud tracks.
/// </summary>
public class TrackClient
{
    private readonly NetHttpClient _http;
    private readonly SoundCloudClient _client;

    private readonly Regex SingleTrackRegex = new(@"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+");
    private readonly Regex TracksRegex = new(@"soundcloud\..+?\/(.*?)\/track");

    /// <summary>
    /// Initializes an instance of <see cref="TrackClient"/>.
    /// </summary>
    public TrackClient(SoundCloudClient client, HttpClient http)
    {
        _http = new NetHttpClient(http);
        _client = client;
    }

    /// <summary>
    /// Checks for valid track(s) url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public bool IsUrlValid(string url)
    {
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        if (isUrl && (TracksRegex.IsMatch(url) || SingleTrackRegex.IsMatch(url)))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    public async ValueTask<TrackInformation?> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var resolvedJson = await _client.ResolveSoundcloudUrlAsync
            (url, cancellationToken);

        return JsonConvert.DeserializeObject<TrackInformation>(resolvedJson)!;
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    public async ValueTask<string?> GetUrlByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var trackInfoJson = await _http.GetAsync($"{Constants.TrackEndpoint}/{trackId}?client_id={_client.ClientId}", cancellationToken);
        if (trackInfoJson is null)
            return null;

        var track = JsonConvert.DeserializeObject<TrackInformation>(trackInfoJson);
        if (track is null || track.PermalinkUrl is null)
            return null;

        return track.PermalinkUrl?.ToString();
    }

    /// <summary>
    /// Gets the metadata associated with the specified track.
    /// </summary>
    public async ValueTask<TrackInformation?> GetByIdAsync(
        long trackId,
        CancellationToken cancellationToken = default)
    {
        var trackUrl = await GetUrlByIdAsync(trackId, cancellationToken);
        if (trackUrl is null)
        {
            return null;
        }

        return await GetAsync(trackUrl, cancellationToken);
    }

    /// <summary>
    /// Enumerates batches of tracks included in the specified url.
    /// </summary>
    public async IAsyncEnumerable<Batch<TrackInformation>> GetTrackBatchesAsync(
        string url,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var resolvedJson = await _client.ResolveSoundcloudUrlAsync
            (url, cancellationToken);

        if (!TracksRegex.IsMatch(url))
        {
            var tracks = new List<TrackInformation>
            {
                JsonConvert.DeserializeObject<TrackInformation>(resolvedJson)!
            };

            yield return Batch.Create(tracks);
            yield break;
        }

        var user = JsonConvert.DeserializeObject<User>(resolvedJson);

        var next_href = default(string?);

        do
        {
            var tracks = new List<TrackInformation>();

            if (user is null)
            {
                break;
            }

            //url = $"https://api-v2.soundcloud.com/users/{user.Id}/tracks?offset=2014-08-15T00&limit=20&representation=&client_id=eLWKwhCY4BrKxcpiyhjyr6SeHiszzUq6&app_locale=en";
            url = next_href ?? $"https://api-v2.soundcloud.com/users/{user.Id}/tracks?limit=200&client_id={_client.ClientId}";

            var tracksJson = await _http.GetAsync(url, cancellationToken);
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

            next_href += $"&client_id={_client.ClientId}";
        } while (true);
    }

    /// <summary>
    /// Enumerates tracks included in the specified url.
    /// </summary>
    public IAsyncEnumerable<TrackInformation> GetTracksAsync(
        string url,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, cancellationToken).FlattenAsync();
}