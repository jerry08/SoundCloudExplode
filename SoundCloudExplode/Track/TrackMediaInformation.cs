using Newtonsoft.Json;

namespace SoundCloudExplode.Track;

public class TrackMediaInformation
{
    [JsonProperty("url")]
    public string? Url { get; set; }
}