using SoundCloudExplode.Playlists;

namespace SoundCloudExplode.Search;

public class PlaylistSearchResult : Playlist, ISearchResult
{
    public string? Url => PermalinkUrl?.ToString();
}