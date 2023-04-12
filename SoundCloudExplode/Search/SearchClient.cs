using System;
using System.Net.Http;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Utils.Extensions;
using System.Text.Json.Nodes;
using System.Text.Json;

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
    /// Checks for valid track(s) url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public bool IsUrlValid(string url)
    {
        url = url.ToLower();
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        return isUrl;
    }

    /// <summary>
    /// Enumerates batches of search results returned by the specified query.
    /// </summary>
    public async IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        SearchFilter searchFilter,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (limit is < Constants.MinLimit or > Constants.MaxLimit)
            throw new SoundcloudExplodeException($"Limit must be between {Constants.MinLimit} and {Constants.MaxLimit}");

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

        //Generic
        var url = $"https://api-v2.soundcloud.com/search{searchFilterStr}?q={Uri.EscapeUriString(searchQuery)}&client_id={_endpoint.ClientId}&limit={limit}&offset={offset}";

        var response = await _http.ExecuteGetAsync(url, cancellationToken);

        var data = JsonNode.Parse(response)!["collection"]!.AsArray();

        foreach (var item in data)
        {
            if (item is null)
                continue;

            var permalinkUrl = item["permalink_url"]?.ToString();
            if (permalinkUrl is null || !Uri.IsWellFormedUriString(permalinkUrl, UriKind.Absolute))
                continue;

            var permalinkUri = new Uri(permalinkUrl);

            //User
            if (permalinkUri.Segments.Length == 2)
            {
                var user = JsonSerializer.Deserialize<UserSearchResult>(item.ToString())!;
                results.Add(user);
                continue;
            }

            //Track
            if (permalinkUri.Segments.Length == 3)
            {
                var track = JsonSerializer.Deserialize<TrackSearchResult>(item.ToString())!;
                results.Add(track);
                continue;
            }

            //Playlist/Album
            if (permalinkUri.Segments.Length == 4 &&
                permalinkUri.Segments[2] == "sets/")
            {
                var playlist = JsonSerializer.Deserialize<PlaylistSearchResult>(item.ToString())!;
                results.Add(playlist);
            }
        }

        yield return Batch.Create(results);
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
    /// Enumerates search results returned by the specified query.
    /// </summary>
    public IAsyncEnumerable<ISearchResult> GetResultsAsync(
        string searchQuery,
        SearchFilter searchFilter = SearchFilter.None,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetResultBatchesAsync(searchQuery, searchFilter, offset, limit, cancellationToken).FlattenAsync();

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
    /// Enumerates playist/album search results returned by the specified query.
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
}