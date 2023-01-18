using SoundCloudExplode.Common;

namespace SoundCloudExplode.Search;

/// <summary>
/// <p>
///     Abstract result returned by a search query.
///     Use pattern matching to handle specific instances of this type.
/// </p>
/// <p>
///     Can be either one of the following:
///     <list type="bullet">
///         <item><see cref="TrackSearchResult" /></item>
///         <item><see cref="PlaylistSearchResult" /></item>
///     </list>
/// </p>
/// </summary>

public interface ISearchResult : IBatchItem
{
    /// <summary>
    /// Result URL.
    /// </summary>
    string? Url { get; }

    /// <summary>
    /// Result title.
    /// </summary>
    string? Title { get; }
}