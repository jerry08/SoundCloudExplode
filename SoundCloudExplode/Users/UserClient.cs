﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Playlists;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Users;

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
    public UserClient(HttpClient http, SoundcloudEndpoint endpoint)
    {
        _http = http;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Checks for valid user url.
    /// </summary>
    /// <param name="url"></param>
    /// <exception cref="SoundcloudExplodeException"/>
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
    /// <exception cref="SoundcloudExplodeException"/>
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
    /// Gets tracks included in the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async IAsyncEnumerable<Batch<Track>> GetTrackBatchesAsync(
        string url,
        UserTrackSortBy trackQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        if (user is null)
            yield break;

        var queryPart = trackQuery switch
        {
            UserTrackSortBy.Popular => "toptracks",
            _ => "tracks"
        };

        var nextUrl = default(string?);

        while (true)
        {
            url = nextUrl ?? $"https://api-v2.soundcloud.com/users/{user.Id}/{queryPart}?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}";

            var response = await _http.ExecuteGetAsync(url, cancellationToken);

            var doc = JsonDocument.Parse(response).RootElement;
            var collectionStr = doc.GetProperty("collection").ToString();

            if (string.IsNullOrEmpty(collectionStr))
                break;

            if (JsonSerializer.Deserialize<List<Track>>(collectionStr)
                is not List<Track> list
                || !list.Any())
            {
                break;
            }

            yield return Batch.Create(list);

            nextUrl = doc.GetProperty("next_href").GetString();

            if (string.IsNullOrEmpty(nextUrl))
                break;

            nextUrl += $"&client_id={_endpoint.ClientId}";
        }
    }

    /// <summary>
    /// Enumerates track results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Track> GetTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, UserTrackSortBy.Default, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates track results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Track> GetTracksAsync(
        string url,
        CancellationToken cancellationToken) =>
        GetTrackBatchesAsync(url, UserTrackSortBy.Default, cancellationToken: cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates popular track results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Track> GetPopularTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, UserTrackSortBy.Popular, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates popular track results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Track> GetPopularTracksAsync(
        string url,
        CancellationToken cancellationToken) =>
        GetTrackBatchesAsync(url, UserTrackSortBy.Popular, cancellationToken: cancellationToken).FlattenAsync();

    /// <summary>
    /// Gets tracks included in the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async IAsyncEnumerable<Batch<Playlist>> GetPlaylistBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        if (user is null)
            yield break;

        var nextUrl = default(string?);

        while (true)
        {
            url = nextUrl ?? $"https://api-v2.soundcloud.com/users/{user.Id}/playlists?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}";

            var response = await _http.ExecuteGetAsync(url, cancellationToken);

            var doc = JsonDocument.Parse(response).RootElement;
            var collectionStr = doc.GetProperty("collection").ToString();

            if (string.IsNullOrEmpty(collectionStr))
                break;

            if (JsonSerializer.Deserialize<List<Playlist>>(collectionStr)
                is not List<Playlist> list
                || !list.Any())
            {
                break;
            }

            yield return Batch.Create(list);

            nextUrl = doc.GetProperty("next_href").GetString();

            if (string.IsNullOrEmpty(nextUrl))
                break;

            nextUrl += $"&client_id={_endpoint.ClientId}";
        }
    }

    /// <summary>
    /// Enumerates playlist results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Playlist> GetPlaylistsAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetPlaylistBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates playlist results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Playlist> GetPlaylistsAsync(
        string url,
        CancellationToken cancellationToken) =>
        GetPlaylistBatchesAsync(url, cancellationToken: cancellationToken).FlattenAsync();

    /// <summary>
    /// Gets tracks included in the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async IAsyncEnumerable<Batch<Playlist>> GetAlbumBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var user = await GetAsync(url, cancellationToken);

        if (user is null)
            yield break;

        var nextUrl = default(string?);

        while (true)
        {
            url = nextUrl ?? $"https://api-v2.soundcloud.com/users/{user.Id}/albums?offset={offset}&limit={limit}&client_id={_endpoint.ClientId}";

            var response = await _http.ExecuteGetAsync(url, cancellationToken);

            var doc = JsonDocument.Parse(response).RootElement;
            var collectionStr = doc.GetProperty("collection").ToString();

            if (string.IsNullOrEmpty(collectionStr))
                break;

            if (JsonSerializer.Deserialize<List<Playlist>>(collectionStr)
                is not List<Playlist> list
                || !list.Any())
            {
                break;
            }

            yield return Batch.Create(list);

            nextUrl = doc.GetProperty("next_href").GetString();

            if (string.IsNullOrEmpty(nextUrl))
                break;

            nextUrl += $"&client_id={_endpoint.ClientId}";
        }
    }

    /// <summary>
    /// Enumerates album results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Playlist> GetAlbumsAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetAlbumBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates album results returned by the specified user url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Playlist> GetAlbumsAsync(
        string url,
        CancellationToken cancellationToken) =>
        GetAlbumBatchesAsync(url, cancellationToken: cancellationToken).FlattenAsync();
}