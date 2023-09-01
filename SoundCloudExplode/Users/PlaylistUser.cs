using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SoundCloudExplode.Playlists;

namespace SoundCloudExplode.Users;

public class PlaylistUser
{
    [JsonPropertyName("avatar_url")]
    public Uri? AvatarUrl { get; set; }

    [JsonPropertyName("city")]
    public object? City { get; set; }

    [JsonPropertyName("comments_count")]
    public long CommentsCount { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonPropertyName("creator_subscriptions")]
    public List<CreatorSubscription> CreatorSubscriptions { get; set; } = new();

    [JsonPropertyName("creator_subscription")]
    public CreatorSubscription? CreatorSubscription { get; set; }

    [JsonPropertyName("description")]
    public object? Description { get; set; }

    [JsonPropertyName("followers_count")]
    public long? FollowersCount { get; set; }

    [JsonPropertyName("followings_count")]
    public long? FollowingsCount { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("groups_count")]
    public long? GroupsCount { get; set; }

    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("last_modified")]
    public DateTimeOffset? LastModified { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("likes_count")]
    public long? LikesCount { get; set; }

    [JsonPropertyName("playlist_likes_count")]
    public long? PlaylistLikesCount { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("permalink_url")]
    public Uri? PermalinkUrl { get; set; }

    [JsonPropertyName("playlist_count")]
    public long? PlaylistCount { get; set; }

    [JsonPropertyName("reposts_count")]
    public object? RepostsCount { get; set; }

    [JsonPropertyName("track_count")]
    public long? TrackCount { get; set; }

    [JsonPropertyName("uri")]
    public Uri? Uri { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("visuals")]
    public Visuals? Visuals { get; set; }

    [JsonPropertyName("badges")]
    public Badges? Badges { get; set; }
}