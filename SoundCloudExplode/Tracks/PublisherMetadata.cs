using System.Text.Json.Serialization;

namespace SoundCloudExplode.Tracks;

public class PublisherMetadata
{
    [JsonPropertyName("id")]
    public long? Id { get; set; }

    [JsonPropertyName("urn")]
    public string? Urn { get; set; }

    [JsonPropertyName("artist")]
    public string? Artist { get; set; }

    [JsonPropertyName("contains_music")]
    public bool ContainsMusic { get; set; }
}