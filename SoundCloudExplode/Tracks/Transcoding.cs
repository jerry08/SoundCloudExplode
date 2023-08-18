using System;
using System.Text.Json.Serialization;

namespace SoundCloudExplode.Tracks;

public class Transcoding
{
    [JsonPropertyName("url")]
    public Uri? Url { get; set; }

    [JsonPropertyName("preset")]
    public string? Preset { get; set; }

    [JsonPropertyName("duration")]
    public long? Duration { get; set; }

    [JsonPropertyName("snipped")]
    public bool Snipped { get; set; }

    [JsonPropertyName("format")]
    public Format? Format { get; set; }

    [JsonPropertyName("quality")]
    public string? Quality { get; set; }
}