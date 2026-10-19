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

    /// <summary>
    /// Gets the file extension for this audio format, or .audio for an adaptive/unknown format.
    /// Downloading preserves the source encoding; it does not convert audio to MP3.
    /// </summary>
    public string GetFileExtension() =>
        Format?.MimeType?.Split(';')[0].Trim().ToLowerInvariant() switch
        {
            "audio/mp4" => ".m4a",
            "audio/mpeg" => ".mp3",
            "audio/ogg" => ".ogg",
            "audio/aac" => ".aac",
            _ => ".audio",
        };
}
