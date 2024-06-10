using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SoundCloudExplode.Tracks;

public class Media
{
    [JsonPropertyName("transcodings")]
    public List<Transcoding> Transcodings { get; set; } = new();
}
