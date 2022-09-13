using Newtonsoft.Json;

namespace SoundCloudExplode.Track;

public partial class TrackMediaInformation
{
    [JsonProperty("url")]
    public string? Url { get; set; }
}