using SoundCloudExplode.Playlist;

namespace SoundCloudExplode.Search;

public class PlaylistSearchResult : PlaylistInformation, ISearchResult
{
    public string? Url => PermalinkUrl?.ToString();
}