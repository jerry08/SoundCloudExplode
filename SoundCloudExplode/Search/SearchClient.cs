using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Search;

/// <summary>
/// Operations related to Soundcloud searching.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="SearchClient"/>.
/// </remarks>
public class SearchClient(HttpClient http, SoundcloudEndpoint endpoint) : ISearchClient
{
    /// <inheritdoc />
    public bool IsUrlValid(string url)
    {
        url = url.ToLower();
        return Uri.IsWellFormedUriString(url, UriKind.Absolute);
    }

    /// <inheritdoc />
    public async ValueTask<List<string>> GetSearchQueriesAsync(
        string query,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        Limitor.Validate(limit);

        var url =
            $"https://api-v2.soundcloud.com/search/queries?q={Uri.EscapeDataString(query)}&client_id=Tl7CY6xVpYugZsGNqmzUhDCRX3urIPNv&limit={limit}&offset={offset}&linked_partitioning=1&app_version=1718276310&app_locale=en";

        var response = await http.ExecuteGetAsync(url, cancellationToken);

        return JsonDocument
            .Parse(response)
            .RootElement.GetProperty("collection")
            .EnumerateArray()
            .Select(x => x.GetProperty("output").GetString()!)
            .ToList();
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        SearchFilter searchFilter,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
    )
    {
        Limitor.Validate(limit);

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
            _ => "",
        };

        while (true)
        {
            results.Clear();

            // Any result
            var url =
                $"https://api-v2.soundcloud.com/search{searchFilterStr}?q={Uri.EscapeDataString(searchQuery)}&client_id={endpoint.ClientId}&limit={limit}&offset={continuationOffset}";

            var response = await http.ExecuteGetAsync(url, cancellationToken);

            var data = JsonDocument
                .Parse(response)
                .RootElement.GetProperty("collection")
                .EnumerateArray();

            foreach (var item in data)
            {
                var permalinkUrl = item.GetProperty("permalink_url").ToString();
                if (
                    permalinkUrl is null
                    || !Uri.IsWellFormedUriString(permalinkUrl, UriKind.Absolute)
                )
                {
                    continue;
                }

                // Don't yield the same result twice
                if (!encounteredUrls.Add(permalinkUrl))
                    continue;

                var permalinkUri = new Uri(permalinkUrl);

                // User result
                if (permalinkUri.Segments.Length == 2)
                {
                    var user = JsonSerializer.Deserialize(
                        item.ToString()!,
                        SourceGenerationContext.Default.UserSearchResult
                    )!;
                    results.Add(user);
                    continue;
                }

                // Track result
                if (permalinkUri.Segments.Length == 3)
                {
                    var track = JsonSerializer.Deserialize(
                        item.ToString()!,
                        SourceGenerationContext.Default.TrackSearchResult
                    )!;
                    results.Add(track);
                    continue;
                }

                // Playlist/Album result
                if (permalinkUri.Segments.Length == 4 && permalinkUri.Segments[2] == "sets/")
                {
                    var playlist = JsonSerializer.Deserialize(
                        item.ToString()!,
                        SourceGenerationContext.Default.PlaylistSearchResult
                    )!;
                    results.Add(playlist);
                }
            }

            if (results.Count == 0)
                break;

            yield return Batch.Create(results);

            continuationOffset += results.Count;
        }
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) => GetResultBatchesAsync(searchQuery, SearchFilter.None, offset, limit, cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.None, cancellationToken: cancellationToken);

    /// <inheritdoc />
    public IAsyncEnumerable<ISearchResult> GetResultsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) => GetResultBatchesAsync(searchQuery, offset, limit, cancellationToken).FlattenAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<ISearchResult> GetResultsAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) => GetResultBatchesAsync(searchQuery, cancellationToken).FlattenAsync();

    /// <inheritdoc />
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Playlist, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) =>
        GetResultBatchesAsync(
                searchQuery,
                SearchFilter.Playlist,
                cancellationToken: cancellationToken
            )
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsWithoutAlbumsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) =>
        GetResultBatchesAsync(
                searchQuery,
                SearchFilter.PlaylistWithoutAlbums,
                offset,
                limit,
                cancellationToken
            )
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsWithoutAlbumsAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) =>
        GetResultBatchesAsync(
                searchQuery,
                SearchFilter.PlaylistWithoutAlbums,
                cancellationToken: cancellationToken
            )
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<TrackSearchResult> GetTracksAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Track, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<TrackSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<TrackSearchResult> GetTracksAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Track, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<TrackSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<UserSearchResult> GetUsersAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.User, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<UserSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<UserSearchResult> GetUsersAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.User, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<UserSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<PlaylistSearchResult> GetAlbumsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Album, offset, limit, cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();

    /// <inheritdoc />
    public IAsyncEnumerable<PlaylistSearchResult> GetAlbumsAsync(
        string searchQuery,
        CancellationToken cancellationToken
    ) =>
        GetResultBatchesAsync(searchQuery, SearchFilter.Album, cancellationToken: cancellationToken)
            .FlattenAsync()
            .OfTypeAsync<PlaylistSearchResult>();
}
