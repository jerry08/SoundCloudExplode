using System;
using SoundCloudExplode.Tracks;

namespace SoundCloudExplode.Tests;

internal static class TestTracks
{
    public static Track Create(
        string protocol = "hls",
        string mimeType = "audio/mp4",
        string? quality = "lq"
    ) =>
        new()
        {
            Policy = "ALLOW",
            Media = new Media
            {
                Transcodings =
                [
                    new Transcoding
                    {
                        Url = new Uri("https://api-v2.soundcloud.com/media/test"),
                        Quality = quality,
                        Format = new Format { Protocol = protocol, MimeType = mimeType },
                    },
                ],
            },
        };
}
