﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using SoundCloudExplode.Common;

namespace SoundCloudExplode.Users;

public class User : IBatchItem
{
    [JsonPropertyName("avatar_url")]
    public Uri? AvatarUrl { get; set; }

    [JsonPropertyName("first_name")]
    public string? FirstName { get; set; }

    [JsonPropertyName("full_name")]
    public string? FullName { get; set; }

    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("kind")]
    public Kind Kind { get; set; }

    [JsonPropertyName("last_modified")]
    public DateTimeOffset? LastModified { get; set; }

    [JsonPropertyName("last_name")]
    public string? LastName { get; set; }

    [JsonPropertyName("permalink")]
    public string? Permalink { get; set; }

    [JsonPropertyName("permalink_url")]
    public string? PermalinkUrl { get; set; }

    [JsonPropertyName("uri")]
    public Uri? Uri { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("country_code")]
    public string? CountryCode { get; set; }

    [JsonPropertyName("badges")]
    public Badges? Badges { get; set; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string ToString() =>
        $"User ({(!string.IsNullOrWhiteSpace(FullName) ? FullName : Username)})";
}
