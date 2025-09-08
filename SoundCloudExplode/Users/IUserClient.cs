using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Playlists;
using SoundCloudExplode.Tracks;

namespace SoundCloudExplode.Users;

/// <summary>
/// Defines the functionality for SoundCloud user operations.
/// </summary>
public interface IUserClient
{
    /// <summary>
    /// Checks for a valid user URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    bool IsUrlValid(string url);

    /// <summary>
    /// Checks for a valid user likes URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    bool IsLikesUrlValid(string url);

    /// <summary>
    /// Gets the metadata for a user.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="SoundcloudExplodeException"/>
    ValueTask<User> GetAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enumerates batches of tracks from a user.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="trackQuery">The sorting criteria for tracks.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Batch<Track>> GetTrackBatchesAsync(
        string url,
        UserTrackSortBy trackQuery,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates track results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates track results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetTracksAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates popular track results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetPopularTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates popular track results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetPopularTracksAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Enumerates liked track results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetLikedTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates liked track results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetLikedTracksAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Gets tracks included in the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Batch<Playlist>> GetPlaylistBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates playlist results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Playlist> GetPlaylistsAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates playlist results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Playlist> GetPlaylistsAsync(string url, CancellationToken cancellationToken);

    /// <summary>
    /// Gets tracks included in the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Batch<Playlist>> GetAlbumBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates album results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Playlist> GetAlbumsAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates album results returned by the specified user URL.
    /// </summary>
    /// <param name="url">The user URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Playlist> GetAlbumsAsync(string url, CancellationToken cancellationToken);
}
