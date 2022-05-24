using Newtonsoft.Json;

namespace SoundCloudExplode.Models.SoundCloud
{
    public partial class TrackMediaInformation
    {
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
