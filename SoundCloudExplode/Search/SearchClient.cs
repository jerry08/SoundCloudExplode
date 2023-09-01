using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Search;

/// <summary>
/// Operations related to Soundcloud searching.
/// </summary>
public class SearchClient
{
    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    /// <summary>
    /// Initializes an instance of <see cref="SearchClient"/>.
    /// </summary>
    public SearchClient(HttpClient http, SoundcloudEndpoint endpoint)
    {
        _http = http;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Checks for valid track(s) url.
    /// </summary>
    /// <param name="url"></param>
    /// <exception cref="SoundcloudExplodeException"/>
    public bool IsUrlValid(string url)
    {
        url = url.ToLower();
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }

    /// <summary>
    /// Enumerates batches of search results returned by the specified query.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        SearchFilter searchFilter,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

        var encounteredUrls = new HashSet<string>(StringComparer.Ordinal);
        var continuationOffset = offset;

        var results = new List<ISearchResult>();

        var searchFilterStr = searchFilter switch
        {
            SearchFilter.Track => "/tracks",
            SearchFilter.Playlist => "/playlists",
            SearchFilter.PlaylistWithoutAlbums => "/playlists_without_albums",
            SearchFilter.Album => "/albums",
            SearchFilter.User => "/users",
            _ => ""
        };

        while(true)
        {
            results.Clear();

            // Any result
            var url = $"https://api-v2.soundcloud.com/search{searchFilterStr}?q={Uri.EscapeDataString(searchQuery)}&client_id={_endpoint.ClientId}&limit={limit}&offset={continuationOffset}";

            var response = await _http.ExecuteGetAsync(url, cancellationToken);

            var data = JsonDocument.Parse(response).RootElement.GetProperty("collection").EnumerateArray();

            foreach (var item in data)
            {
                var permalinkUrl = item.GetProperty("permalink_url").ToString();
                if (permalinkUrl is null || !Uri.IsWellFormedUriString(permalinkUrl, UriKind.Absolute))
                    continue;

                // Don't yield the same result twice
                if (!encounteredUrls.Add(permalinkUrl))
                    continue;

                var permalinkUri = new Uri(permalinkUrl);

                // User result
                if (permalinkUri.Segments.Length == 2)
                {
                    var user = JsonSerializer.Deserialize<UserSearchResult>(item.ToString()!)!;
                    results.Add(user);
                    continue;
                }

                // Track result
                if (permalinkUri.Segments.Length == 3)
                {
                    var track = JsonSerializer.Deserialize<TrackSearchResult>(item.ToString()!)!;
                    results.Add(track);
                    continue;
                }

                // Playlist/Album result
                if (permalinkUri.Segments.Length == 4 &&
                    permalinkUri.Segments[2] == "sets/")
                {
                    var playlist = JsonSerializer.Deserialize<PlaylistSearchResult>(item.ToString()!)!;
                    results.Add(playlist);
                }
            }

            if (results.Count == 0)
                break;

            yield return Batch.Create(results);

            continuationOffset += results.Count;
        }
    }

    /// <summary>
    /// Enumerates batches of search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.None, offset, limit, cancellationToken);

    /// <summary>
    /// Enumerates batches of search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.None, cancellationToken: cancellationToken);

    /// <summary>
    /// Enumerates search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<ISearchResult> GetResultsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<ISearchResult> GetResultsAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates playlist search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Playlist, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <summary>
    /// Enumerates playlist search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Playlist, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <summary>
    /// Enumerates playlists without albums search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsWithoutAlbumsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.PlaylistWithoutAlbums, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <summary>
    /// Enumerates playlists without albums search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsWithoutAlbumsAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.PlaylistWithoutAlbums, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <summary>
    /// Enumerates track search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<TrackSearchResult> GetTracksAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Track, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<TrackSearchResult>();

    /// <summary>
    /// Enumerates track search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<TrackSearchResult> GetTracksAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Track, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<TrackSearchResult>();

    /// <summary>
    /// Enumerates user search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<UserSearchResult> GetUsersAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.User, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<UserSearchResult>();

    /// <summary>
    /// Enumerates user search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<UserSearchResult> GetUsersAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.User, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<UserSearchResult>();

    /// <summary>
    /// Enumerates album search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetAlbumsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Album, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <summary>
    /// Enumerates album search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<PlaylistSearchResult> GetAlbumsAsync(
        string searchQuery,
        CancellationToken cancellationToken) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Album, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();
}