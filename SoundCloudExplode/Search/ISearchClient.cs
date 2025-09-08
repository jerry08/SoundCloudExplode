using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Search;

/// <summary>
/// Provides functionality to search SoundCloud for tracks, playlists, albums, and users.
/// </summary>
public interface ISearchClient
{
    /// <summary>
    /// Checks if a given URL is a well-formed SoundCloud URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the URL is valid; otherwise, false.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    bool IsUrlValid(string url);

    /// <summary>
    /// Gets a list of suggested search queries related to the specified query.
    /// </summary>
    /// <param name="query">The query string.</param>
    /// <param name="offset">The offset of results (pagination).</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of suggested search queries.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    ValueTask<List<string>> GetSearchQueriesAsync(
        string query,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates batches of search results by query and filter type.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="searchFilter">The type of search filter (track, user, playlist, etc.).</param>
    /// <param name="offset">The offset of results (pagination).</param>
    /// <param name="limit">The maximum number of results per batch.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of result batches.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<Batch<ISearchResult>> GetResultBatchesAsync(
        string searchQuery,
        SearchFilter searchFilter,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates all search results by query without a specific filter.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="offset">Offset for pagination.</param>
    /// <param name="limit">Limit for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of raw search results.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<ISearchResult> GetResultsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets track search results for the specified query.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="offset">Offset for pagination.</param>
    /// <param name="limit">Limit for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of <see cref="TrackSearchResult"/> objects.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<TrackSearchResult> GetTracksAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets playlist search results for the specified query.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="offset">Offset for pagination.</param>
    /// <param name="limit">Limit for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of <see cref="PlaylistSearchResult"/> objects.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets playlist results excluding albums for the specified query.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="offset">Offset for pagination.</param>
    /// <param name="limit">Limit for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of playlists without albums.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<PlaylistSearchResult> GetPlaylistsWithoutAlbumsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets album search results for the specified query.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="offset">Offset for pagination.</param>
    /// <param name="limit">Limit for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of <see cref="PlaylistSearchResult"/> representing albums.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<PlaylistSearchResult> GetAlbumsAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets user search results for the specified query.
    /// </summary>
    /// <param name="searchQuery">The search query.</param>
    /// <param name="offset">Offset for pagination.</param>
    /// <param name="limit">Limit for pagination.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of <see cref="UserSearchResult"/> objects.</returns>
    /// <exception cref="SoundcloudExplodeException"/>
    IAsyncEnumerable<UserSearchResult> GetUsersAsync(
        string searchQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );
}
