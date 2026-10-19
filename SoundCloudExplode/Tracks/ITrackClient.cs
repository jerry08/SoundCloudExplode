using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Tracks;

/// <summary>
/// Defines the functionality for SoundCloud track operations.
/// </summary>
public interface ITrackClient
{
    /// <summary>
    /// Checks for a valid track URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="SoundcloudExplodeException"/>
    Task<bool> IsUrlValidAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the metadata associated with the specified track URL.
    /// </summary>
    /// <param name="url">The track URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="SoundcloudExplodeException"/>
    ValueTask<Track?> GetAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the URL of a track by its ID.
    /// </summary>
    /// <param name="trackId">The ID of the track.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="SoundcloudExplodeException"/>
    ValueTask<string?> GetUrlByIdAsync(long trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the metadata for a track by its ID.
    /// </summary>
    /// <param name="trackId">The ID of the track.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="SoundcloudExplodeException"/>
    ValueTask<Track?> GetByIdAsync(long trackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a progressive audio URL or an HLS playlist URL for a track from its URL.
    /// </summary>
    /// <param name="url">The track URL.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="SoundcloudExplodeException"/>
    ValueTask<string?> GetDownloadUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a progressive audio URL or an HLS playlist URL for a track.
    /// </summary>
    /// <param name="track">The track object.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <exception cref="TrackUnavailableException"/>
    ValueTask<string?> GetDownloadUrlAsync(
        Track track,
        CancellationToken cancellationToken = default
    );
}
