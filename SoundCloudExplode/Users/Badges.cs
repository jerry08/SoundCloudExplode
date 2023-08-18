using System.Text.Json.Serialization;

namespace SoundCloudExplode.Users;

public class Badges
{
    [JsonPropertyName("pro_unlimited")]
    public bool ProUnlimited { get; set; }

    [JsonPropertyName("verified")]
    public bool Verified { get; set; }
}