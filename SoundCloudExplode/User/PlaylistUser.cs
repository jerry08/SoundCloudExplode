using System;
using Newtonsoft.Json;
using SoundCloudExplode.Playlist;

namespace SoundCloudExplode.User;

public class PlaylistUser
{
    [JsonProperty("avatar_url")]
    public Uri? AvatarUrl { get; set; }

    public object? City { get; set; }

    [JsonProperty("comments_count")]
    public long CommentsCount { get; set; }

    [JsonProperty("country_code")]
    public string? CountryCode { get; set; }

    [JsonProperty("created_at")]
    public DateTimeOffset? CreatedAt { get; set; }

    [JsonProperty("creator_subscriptions")]
    public CreatorSubscription[]? CreatorSubscriptions { get; set; }

    [JsonProperty("creator_subscription")]
    public CreatorSubscription? CreatorSubscription { get; set; }

    public object? Description { get; set; }

    [JsonProperty("followers_count")]
    public long? FollowersCount { get; set; }

    [JsonProperty("followings_count")]
    public long? FollowingsCount { get; set; }

    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [JsonProperty("full_name")]
    public string? FullName { get; set; }

    [JsonProperty("groups_count")]
    public long? GroupsCount { get; set; }

    public long? Id { get; set; }

    public string? Kind { get; set; }

    [JsonProperty("last_modified")]
    public DateTimeOffset? LastModified { get; set; }

    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("likes_count")]
    public long? LikesCount { get; set; }

    [JsonProperty("playlist_likes_count")]
    public long? PlaylistLikesCount { get; set; }

    [JsonProperty("permalink")]
    public string? Permalink { get; set; }

    [JsonProperty("permalink_url")]
    public Uri? PermalinkUrl { get; set; }

    [JsonProperty("playlist_count")]
    public long? PlaylistCount { get; set; }

    [JsonProperty("reposts_count")]
    public object? RepostsCount { get; set; }

    [JsonProperty("track_count")]
    public long? TrackCount { get; set; }

    public Uri? Uri { get; set; }

    public string? Urn { get; set; }

    public string? Username { get; set; }

    public bool Verified { get; set; }

    public Visuals? Visuals { get; set; }

    public Badges? Badges { get; set; }
}