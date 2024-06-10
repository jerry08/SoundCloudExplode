using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SoundCloudExplode.Common;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Users;

namespace SoundCloudExplode.Playlists;

public class Playlist : IBatchItem
{
    [JsonPropertyName("artwork_url")]
    public Uri? ArtworkUrl { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    [JsonPropertyName("embeddable_by")]
    public string? EmbeddableBy { get; set; }

    [JsonPropertyName("genre")]
    public string? Genre { get; set; }

    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("label_name")]
    public object? LabelName { get; set; }

    [JsonPropertyName("last_modified")]
    public DateTimeOffset LastModified { get; set; }

    [JsonPropertyName("license")]
    public string? License { get; set; }

    [JsonPropertyName("likes_count")]
    public long LikesCount { get; set; }

    [JsonPropertyName("managed_by_feeds")]
    public bool ManagedByFeeds { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("permalink_url")]
    public Uri? PermalinkUrl { get; set; }

    [JsonPropertyName("public")]
    public bool Public { get; set; }

    [JsonPropertyName("purchase_title")]
    public object? PurchaseTitle { get; set; }

    [JsonPropertyName("purchase_url")]
    public object? PurchaseUrl { get; set; }

    [JsonPropertyName("release_date")]
    public object? ReleaseDate { get; set; }

    [JsonPropertyName("reposts_count")]
    public long RepostsCount { get; set; }

    [JsonPropertyName("secret_token")]
    public object? SecretToken { get; set; }

    [JsonPropertyName("sharing")]
    public string? Sharing { get; set; }

    [JsonPropertyName("tag_list")]
    public string? TagList { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("uri")]
    public Uri? Uri { get; set; }

    [JsonPropertyName("user_id")]
    public long UserId { get; set; }

    [JsonPropertyName("set_type")]
    public string? SetType { get; set; }

    [JsonPropertyName("is_album")]
    public bool IsAlbum { get; set; }

    [JsonPropertyName("published_at")]
    public object? PublishedAt { get; set; }

    [JsonPropertyName("display_date")]
    public DateTimeOffset DisplayDate { get; set; }

    [JsonPropertyName("user")]
    public PlaylistUser? User { get; set; }

    [JsonPropertyName("tracks")]
    public List<Track> Tracks { get; set; } = new();

    [JsonPropertyName("track_count")]
    public long? TrackCount { get; set; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Playlist/Album ({Title})";
}
