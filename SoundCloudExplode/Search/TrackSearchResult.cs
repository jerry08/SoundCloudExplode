using SoundCloudExplode.Track;

namespace SoundCloudExplode.Search;

public class TrackSearchResult : TrackInformation, ISearchResult
{
    public string? Url => PermalinkUrl?.ToString();
}