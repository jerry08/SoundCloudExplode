namespace SoundCloudExplode.Search;

/// <summary>
/// Filter applied to a Soundcloud search query.
/// </summary>
public enum SearchFilter
{
    /// <summary>
    /// No filter applied.
    /// </summary>
    None,

    /// <summary>
    /// Only search for tracks.
    /// </summary>
    Track,

    /// <summary>
    /// Only search for playlists.
    /// </summary>
    Playlist,

    /// <summary>
    /// Only search for playlists without albums.
    /// </summary>
    PlaylistWithoutAlbums,

    /// <summary>
    /// Only search for albums.
    /// </summary>
    Album,

    /// <summary>
    /// Only search for users.
    /// </summary>
    User
}