using System.Text.Json.Serialization;

namespace SoundCloudExplode.Playlists;

public class Product
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}
