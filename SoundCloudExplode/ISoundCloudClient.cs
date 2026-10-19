using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Common;
using SoundCloudExplode.Playlists;
using SoundCloudExplode.Search;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Users;

namespace SoundCloudExplode;

/// <summary>
/// Interface for SoundCloud client operations.
/// </summary>
public interface ISoundCloudClient
{
    /// <summary>
    /// Gets a value indicating whether the client is initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Operations related to Soundcloud search.
    /// </summary>
    ISearchClient Search { get; }

    /// <summary>
    /// Operations related to Soundcloud tracks.
    /// </summary>
    ITrackClient Tracks { get; }

    /// <summary>
    /// Operations related to Soundcloud playlists/albums.
    /// </summary>
    IPlaylistClient Playlists { get; }

    /// <summary>
    /// Operations related to Soundcloud users.
    /// </summary>
    IUserClient Users { get; }

    /// <summary>
    /// Initializes the client asynchronously.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the client ID.
    /// </summary>
    Task<string> GetClientIdAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the kind of a SoundCloud URL.
    /// </summary>
    Task<Kind> GetUrlKindAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads progressive audio or an unencrypted, on-demand HLS stream to a file path.
    /// Preserves the source encoding; use Track.GetPreferredTranscoding().GetFileExtension()
    /// to choose an appropriate extension.
    /// </summary>
    ValueTask DownloadAsync(
        Track track,
        string filePath,
        IProgress<double>? progress = null,
        Dictionary<string, string>? headers = null,
        CancellationToken cancellationToken = default
    );
}
