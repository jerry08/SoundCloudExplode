namespace SoundCloudExplode.Search;

public class UserSearchResult : User.User, ISearchResult
{
    public string? Url => PermalinkUrl?.ToString();

    public string? Title => Username;
}