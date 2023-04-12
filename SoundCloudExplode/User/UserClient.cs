using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Playlist;
using SoundCloudExplode.Track;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.User;

/// <summary>
/// Operations related to Soundcloud user.
/// (Note: Everything for Playlists and Albums are handled the same.)
/// </summary>
public class UserClient
{
    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    /// <summary>
    /// Initializes an instance of <see cref="UserClient"/>.
    /// </summary>
    public UserClient(
        HttpClient http,
        SoundcloudEndpoint endpoint)
    {
        _http = http;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Checks for valid user url
    /// </summary>
    /// <param name="url"></param>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public bool IsUrlValid(string url)
    {
        url = url.ToLower();

        if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            return false;

        var builder = new UriBuilder(url);

        return (builder.Host == "soundcloud.com" || builder.Host == "m.soundcloud.com") &&
            builder.Uri.Segments.Length == 2;
    }

    /// <summary>
    /// Gets the metadata associated with the specified user.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public async ValueTask<User> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid user url");

        var resolvedJson = await _endpoint.ResolveUrlAsync(url, cancellationToken);
        return JsonSerializer.Deserialize<User>(resolvedJson)!;
    }

    /// <summary>
    /// Enumerates batches of tracks included in the specified user.
    /// </summary>
    public async IAsyncEnumerable<Batch<TrackInformation>> GetTrackBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        var response = await _http.ExecuteGetAsync(
            $"https://api-v2.soundcloud.com/users/{user.Id}/toptracks?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}",
            cancellationToken);

        var data = JsonNode.Parse(response)!["collection"]!.ToString();

        yield return Batch.Create(JsonSerializer.Deserialize<List<TrackInformation>>(data)!);
    }

    /// <summary>
    /// Enumerates tracks included in the specified user url.
    /// </summary>
    public IAsyncEnumerable<TrackInformation> GetTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates batches of popular tracks included in the specified user.
    /// </summary>
    public async IAsyncEnumerable<Batch<TrackInformation>> GetPopularTrackBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        var response = await _http.ExecuteGetAsync(
            $"https://api-v2.soundcloud.com/users/{user.Id}/tracks?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}",
            cancellationToken);

        var data = JsonNode.Parse(response)!["collection"]!.ToString();

        yield return Batch.Create(JsonSerializer.Deserialize<List<TrackInformation>>(data)!);
    }

    /// <summary>
    /// Enumerates popular tracks included in the specified user url.
    /// </summary>
    public IAsyncEnumerable<TrackInformation> GetPopularTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetPopularTrackBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates playlists of tracks included in the specified user.
    /// </summary>
    public async IAsyncEnumerable<Batch<PlaylistInformation>> GetPlaylistBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        var response = await _http.ExecuteGetAsync(
            $"https://api-v2.soundcloud.com/users/{user.Id}/playlists?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}",
            cancellationToken);

        var data = JsonNode.Parse(response)!["collection"]!.ToString();

        yield return Batch.Create(JsonSerializer.Deserialize<List<PlaylistInformation>>(data)!);
    }

    /// <summary>
    /// Enumerates playlists included in the specified user url.
    /// </summary>
    public IAsyncEnumerable<PlaylistInformation> GetPlaylistsAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetPlaylistBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates batches of albums included in the specified user.
    /// </summary>
    public async IAsyncEnumerable<Batch<PlaylistInformation>> GetAlbumBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        var response = await _http.ExecuteGetAsync(
            $"https://api-v2.soundcloud.com/users/{user.Id}/albums?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}",
            cancellationToken);

        var data = JsonNode.Parse(response)!["collection"]!.ToString();

        yield return Batch.Create(JsonSerializer.Deserialize<List<PlaylistInformation>>(data)!);
    }

    /// <summary>
    /// Enumerates albums included in the specified user url.
    /// </summary>
    public IAsyncEnumerable<PlaylistInformation> GetAlbumsAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetAlbumBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();
}