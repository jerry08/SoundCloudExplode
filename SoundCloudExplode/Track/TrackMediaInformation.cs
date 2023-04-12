using System.Text.Json.Serialization;

namespace SoundCloudExplode.Track;

public class TrackMediaInformation
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}