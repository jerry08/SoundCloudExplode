using System.Text.Json.Serialization;

namespace SoundCloudExplode.Playlists;

public class CreatorSubscription
{
    [JsonPropertyName("product")]
    public Product? Product { get; set; }
}