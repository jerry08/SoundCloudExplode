using SoundCloudExplode.Users;

namespace SoundCloudExplode.Search;

public class UserSearchResult : User, ISearchResult
{
    public string? Url => PermalinkUrl?.ToString();

    public string? Title => Username;
}