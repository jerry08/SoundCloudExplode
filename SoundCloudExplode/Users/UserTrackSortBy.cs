namespace SoundCloudExplode.Users;

/// <summary>
/// Sort applied to a SoundCloud user's tracks.
/// </summary>
public enum UserTrackSortBy
{
    /// <summary>
    /// Default sorting.
    /// </summary>
    Default,

    /// <summary>
    /// Sort by popular tracks.
    /// </summary>
    Popular,

    /// <summary>
    /// Sort by liked tracks.
    /// </summary>
    Likes
}
