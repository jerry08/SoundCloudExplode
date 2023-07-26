using SoundCloudExplode.Tracks;

namespace SoundCloudExplode.Search;

public class TrackSearchResult : Track, ISearchResult
{
    public string? Url => PermalinkUrl?.ToString();
}