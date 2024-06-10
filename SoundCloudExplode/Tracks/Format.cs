using System.Text.Json.Serialization;

namespace SoundCloudExplode.Tracks;

public class Format
{
    [JsonPropertyName("protocol")]
    public string? Protocol { get; set; }

    [JsonPropertyName("mime_type")]
    public string? MimeType { get; set; }
}
