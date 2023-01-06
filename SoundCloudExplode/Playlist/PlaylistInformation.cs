using SoundCloudExplode.Common;
using SoundCloudExplode.Track;
using System;

namespace SoundCloudExplode.Playlist;

public partial class PlaylistInformation
{
    public Uri? ArtworkUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public string? Description { get; set; }
    public long Duration { get; set; }
    public string? EmbeddableBy { get; set; }
    public string? Genre { get; set; }
    public string? Id { get; set; }
    public string? Kind { get; set; }
    public object? LabelName { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? License { get; set; }
    public long LikesCount { get; set; }
    public bool ManagedByFeeds { get; set; }
    public string? Permalink { get; set; }
    public Uri? PermalinkUrl { get; set; }
    public bool Public { get; set; }
    public object? PurchaseTitle { get; set; }
    public object? PurchaseUrl { get; set; }
    public object? ReleaseDate { get; set; }
    public long RepostsCount { get; set; }
    public object? SecretToken { get; set; }
    public string? Sharing { get; set; }
    public string? TagList { get; set; }
    public string? Title { get; set; }
    public Uri? Uri { get; set; }
    public long UserId { get; set; }
    public string? SetType { get; set; }
    public bool IsAlbum { get; set; }
    public object? PublishedAt { get; set; }
    public DateTimeOffset DisplayDate { get; set; }
    public PlaylistInformationUser? User { get; set; }
    //public Track[]? Tracks { get; set; }
    public TrackInformation[]? Tracks { get; set; }
    public long? TrackCount { get; set; }
}

public partial class Media
{
    public Transcoding[]? Transcodings { get; set; }
}

public partial class Transcoding
{
    public Uri? Url { get; set; }
    public string? Preset { get; set; }
    public long? Duration { get; set; }
    public bool Snipped { get; set; }
    public Format? Format { get; set; }
    public Quality Quality { get; set; }
}

public partial class Format
{
    public Protocol? Protocol { get; set; }
    public MimeType? MimeType { get; set; }
}

public partial class TrackUser
{
    public Uri? AvatarUrl { get; set; }
    public string? FirstName { get; set; }
    public string? FullName { get; set; }
    public long? Id { get; set; }
    public string? Kind { get; set; }
    public DateTimeOffset LastModified { get; set; }
    public string? LastName { get; set; }
    public string? Permalink { get; set; }
    public Uri? PermalinkUrl { get; set; }
    public Uri? Uri { get; set; }
    public string? Urn { get; set; }
    public string? Username { get; set; }
    public bool Verified { get; set; }
    public object? City { get; set; }
    public string? CountryCode { get; set; }
    public Badges? Badges { get; set; }
}

public partial class Badges
{
    public bool ProUnlimited { get; set; }
    public bool Verified { get; set; }
}

public partial class PlaylistInformationUser
{
    public Uri? AvatarUrl { get; set; }
    public object? City { get; set; }
    public long CommentsCount { get; set; }
    public string? CountryCode { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public CreatorSubscription[]? CreatorSubscriptions { get; set; }
    public CreatorSubscription? CreatorSubscription { get; set; }
    public object? Description { get; set; }
    public long? FollowersCount { get; set; }
    public long? FollowingsCount { get; set; }
    public string? FirstName { get; set; }
    public string? FullName { get; set; }
    public long? GroupsCount { get; set; }
    public long? Id { get; set; }
    public string? Kind { get; set; }
    public DateTimeOffset? LastModified { get; set; }
    public string? LastName { get; set; }
    public long? LikesCount { get; set; }
    public long? PlaylistLikesCount { get; set; }
    public string? Permalink { get; set; }
    public Uri? PermalinkUrl { get; set; }
    public long? PlaylistCount { get; set; }
    public object? RepostsCount { get; set; }
    public long? TrackCount { get; set; }
    public Uri? Uri { get; set; }
    public string? Urn { get; set; }
    public string? Username { get; set; }
    public bool Verified { get; set; }
    public Visuals? Visuals { get; set; }
    public Badges? Badges { get; set; }
}

public partial class CreatorSubscription
{
    public Product? Product { get; set; }
}

public partial class Product
{
    public string? Id { get; set; }
}

public partial class Visuals
{
    public string? Urn { get; set; }
    public bool Enabled { get; set; }
    public Visual[]? VisualsVisuals { get; set; }
    public object? Tracking { get; set; }
}

public partial class Visual
{
    public string? Urn { get; set; }
    public long? EntryTime { get; set; }
    public Uri? VisualUrl { get; set; }
}

public enum Kind { Track };

public enum MimeType { AudioMpeg, AudioOggCodecsOpus };

public enum Protocol { Hls, Progressive };

public enum Preset { Mp30_0, Opus0_0 };

public enum Quality { Sq };

public enum MonetizationModel { Blackbox };