using System;
using System.Text.Json.Serialization;

namespace SoundCloudExplode.Playlists;

public class Visual
{
    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("entry_time")]
    public long? EntryTime { get; set; }

    [JsonPropertyName("visual_url")]
    public Uri? VisualUrl { get; set; }
}