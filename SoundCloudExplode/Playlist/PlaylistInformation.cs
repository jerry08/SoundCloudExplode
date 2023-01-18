using System;
using Newtonsoft.Json;
using SoundCloudExplode.User;
using SoundCloudExplode.Track;
using SoundCloudExplode.Common;

namespace SoundCloudExplode.Playlist;

public class PlaylistInformation : IBatchItem
{
    [JsonProperty("artwork_url")]
    public Uri? ArtworkUrl { get; set; }

    [JsonProperty("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    public string? Description { get; set; }

    public long Duration { get; set; }

    [JsonProperty("embeddable_by")]
    public string? EmbeddableBy { get; set; }

    public string? Genre { get; set; }

    public string? Id { get; set; }

    public string? Kind { get; set; }

    [JsonProperty("label_name")]
    public object? LabelName { get; set; }

    [JsonProperty("last_modified")]
    public DateTimeOffset LastModified { get; set; }

    public string? License { get; set; }

    [JsonProperty("likes_count")]
    public long LikesCount { get; set; }

    [JsonProperty("managed_by_feeds")]
    public bool ManagedByFeeds { get; set; }

    public string? Permalink { get; set; }

    [JsonProperty("permalink_url")]
    public Uri? PermalinkUrl { get; set; }

    public bool Public { get; set; }

    [JsonProperty("purchase_title")]
    public object? PurchaseTitle { get; set; }

    [JsonProperty("purchase_url")]
    public object? PurchaseUrl { get; set; }

    [JsonProperty("release_date")]
    public object? ReleaseDate { get; set; }

    [JsonProperty("reposts_count")]
    public long RepostsCount { get; set; }

    [JsonProperty("secret_token")]
    public object? SecretToken { get; set; }

    public string? Sharing { get; set; }

    [JsonProperty("tag_list")]
    public string? TagList { get; set; }

    public string? Title { get; set; }

    public Uri? Uri { get; set; }

    [JsonProperty("user_id")]
    public long UserId { get; set; }

    [JsonProperty("set_type")]
    public string? SetType { get; set; }

    [JsonProperty("is_album")]
    public bool IsAlbum { get; set; }

    [JsonProperty("published_at")]
    public object? PublishedAt { get; set; }

    [JsonProperty("display_date")]
    public DateTimeOffset DisplayDate { get; set; }

    public PlaylistUser? User { get; set; }
    //public Track[]? Tracks { get; set; }

    public TrackInformation[]? Tracks { get; set; }

    [JsonProperty("track_count")]
    public long? TrackCount { get; set; }
}

public class Media
{
    public Transcoding[]? Transcodings { get; set; }
}

public class CreatorSubscription
{
    public Product? Product { get; set; }
}

public class Product
{
    public string? Id { get; set; }
}

public class Visuals
{
    public string? Urn { get; set; }

    public bool Enabled { get; set; }

    [JsonProperty("visuals")]
    public Visual[]? Items { get; set; }

    public object? Tracking { get; set; }
}

public class Visual
{
    public string? Urn { get; set; }

    [JsonProperty("entry_time")]
    public long? EntryTime { get; set; }

    [JsonProperty("visual_url")]
    public Uri? VisualUrl { get; set; }
}

public enum Kind { Track };

public enum MimeType { AudioMpeg, AudioOggCodecsOpus };

public enum Protocol { Hls, Progressive };

public enum Preset { Mp30_0, Opus0_0 };

public enum Quality { Sq };

public enum MonetizationModel { Blackbox };