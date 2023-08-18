using System.Text.Json.Serialization;

namespace SoundCloudExplode.Playlists;

public class Visuals
{
    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("visuals")]
    public Visual[]? Items { get; set; }

    [JsonPropertyName("tracking")]
    public object? Tracking { get; set; }
}