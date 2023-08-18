using System.Text.Json.Serialization;

namespace SoundCloudExplode.Tracks;

public class Media
{
    [JsonPropertyName("transcodings")]
    public Transcoding[]? Transcodings { get; set; }
}