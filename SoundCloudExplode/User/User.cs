using System;
using Newtonsoft.Json;
using SoundCloudExplode.Common;

namespace SoundCloudExplode.User;

public class User : IBatchItem
{
    [JsonProperty("avatar_url")]
    public Uri? AvatarUrl { get; set; }

    [JsonProperty("first_name")]
    public string? FirstName { get; set; }

    [JsonProperty("full_name")]
    public string? FullName { get; set; }

    [JsonProperty("id")]
    public long? Id { get; set; }

    [JsonProperty("kind")]
    public string? Kind { get; set; }

    [JsonProperty("last_modified")]
    public DateTimeOffset? LastModified { get; set; }

    [JsonProperty("last_name")]
    public string? LastName { get; set; }

    [JsonProperty("permalink")]
    public string? Permalink { get; set; }

    [JsonProperty("permalink_url")]
    public Uri? PermalinkUrl { get; set; }

    [JsonProperty("uri")]
    public Uri? Uri { get; set; }

    [JsonProperty("urn")]
    public string? Urn { get; set; }

    [JsonProperty("username")]
    public string? Username { get; set; }

    [JsonProperty("verified")]
    public bool Verified { get; set; }

    [JsonProperty("city")]
    public string? City { get; set; }

    [JsonProperty("country_code")]
    public string? CountryCode { get; set; }

    [JsonProperty("badges")]
    public Badges? Badges { get; set; }
}

public class Badges
{
    [JsonProperty("pro_unlimited")]
    public bool ProUnlimited { get; set; }

    [JsonProperty("verified")]
    public bool Verified { get; set; }
}