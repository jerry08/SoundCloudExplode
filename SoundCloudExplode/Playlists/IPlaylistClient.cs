using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Common;
using SoundCloudExplode.Tracks;

namespace SoundCloudExplode.Playlists;

/// <summary>
/// Defines the functionality for SoundCloud playlist/album operations.
/// </summary>
public interface IPlaylistClient
{
    /// <summary>
    /// Checks for a valid playlist URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task<bool> IsUrlValidAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the metadata for a playlist.
    /// </summary>
    /// <param name="url">The playlist URL.</param>
    /// <param name="populateAllTracks">
    ///   Set to true to populate all tracks in the playlist.
    ///   If false, only the tracks id and playlist info will return.
    /// </param>
    /// <param name="cancellationToken">A cancellation token.</param>
    ValueTask<Playlist> GetAsync(
        string url,
        bool populateAllTracks = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates batches of tracks in a playlist.
    /// </summary>
    /// <param name="url">The playlist URL.</param>
    /// <param name="offset">The offset for pagination.</param>
    /// <param name="limit">The limit for the number of results.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Batch<Track>> GetTrackBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Enumerates all tracks in a playlist.
    /// </summary>
    /// <param name="url">The playlist URL.</param>
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
    /// Enumerates all tracks in a playlist.
    /// </summary>
    /// <param name="url">The playlist URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    IAsyncEnumerable<Track> GetTracksAsync(string url, CancellationToken cancellationToken);
}
