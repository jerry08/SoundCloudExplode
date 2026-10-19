using System;
using System.Net.Http;
using System.Threading.Tasks;
using SoundCloudExplode.Tracks;
using Xunit;

namespace SoundCloudExplode.Tests;

public class TrackDownloadSpecs
{
    [Fact]
    public async Task Issue_18_low_quality_aac_has_a_stream_url()
    {
        // The formats returned for issue #18: concrete AAC plus an adaptive stream.
        var track = new Track
        {
            Policy = "ALLOW",
            Media = new Media
            {
                Transcodings =
                [
                    new Transcoding
                    {
                        Url = new Uri("https://api-v2.soundcloud.com/media/aac/stream/hls"),
                        Quality = "lq",
                        Format = new Format
                        {
                            Protocol = "hls",
                            MimeType = "audio/mp4; codecs=\"mp4a.40.2\"",
                        },
                    },
                    new Transcoding
                    {
                        Url = new Uri("https://api-v2.soundcloud.com/media/adaptive/stream/hls"),
                        Quality = "sq",
                        Format = new Format { Protocol = "hls", MimeType = "audio/mpegurl" },
                    },
                ],
            },
        };
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    Assert.Equal("/media/aac/stream/hls", request.RequestUri!.AbsolutePath);
                    return StubHttpMessageHandler.Text(
                        """{"url":"https://audio.example/playlist.m3u8"}"""
                    );
                }
            )
        );
        var client = new SoundCloudClient("test-client", http);

        var url = await client.Tracks.GetDownloadUrlAsync(track);

        Assert.Equal("https://audio.example/playlist.m3u8", url);
    }

    [Theory]
    [InlineData("progressive", "audio/mpeg", "sq", ".mp3")]
    [InlineData("hls", "audio/mpeg", "sq", ".mp3")]
    [InlineData("hls", "audio/ogg; codecs=\"opus\"", "sq", ".ogg")]
    [InlineData("progressive", "audio/mp4", "hq", ".m4a")]
    [InlineData("hls", "audio/mp4; codecs=\"mp4a.40.2\"", "lq", ".m4a")]
    [InlineData("hls", "audio/aac", null, ".aac")]
    [InlineData("hls", "audio/mpegurl", "sq", ".audio")]
    public async Task Supported_formats_produce_playable_urls(
        string protocol,
        string mimeType,
        string? quality,
        string extension
    )
    {
        var track = TestTracks.Create(protocol, mimeType, quality);
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (_, _) => StubHttpMessageHandler.Text("""{"url":"https://audio.example/stream"}""")
            )
        );

        var result = await new SoundCloudClient("test", http).Tracks.GetDownloadUrlAsync(track);

        Assert.Equal("https://audio.example/stream", result);
        Assert.Equal(extension, track.GetPreferredTranscoding()!.GetFileExtension());
    }

    [Fact]
    public async Task Higher_quality_is_preferred_then_progressive_within_the_same_quality()
    {
        var track = TestTracks.Create("progressive", "audio/mpeg", "sq");
        track.Media!.Transcodings.Add(
            new Transcoding
            {
                Url = new Uri("https://api-v2.soundcloud.com/media/hq-hls"),
                Quality = "hq",
                Format = new Format { Protocol = "hls", MimeType = "audio/mp4" },
            }
        );
        var preferred = new Transcoding
        {
            Url = new Uri("https://api-v2.soundcloud.com/media/hq-progressive"),
            Quality = "hq",
            Format = new Format { Protocol = "progressive", MimeType = "audio/mp4" },
        };
        track.Media.Transcodings.Add(preferred);
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    Assert.Equal(preferred.Url.AbsolutePath, request.RequestUri!.AbsolutePath);
                    return StubHttpMessageHandler.Text(
                        """{"url":"https://audio.example/audio.m4a"}"""
                    );
                }
            )
        );

        Assert.Equal(
            "https://audio.example/audio.m4a",
            await new SoundCloudClient("test", http).Tracks.GetDownloadUrlAsync(track)
        );
    }

    [Fact]
    public async Task Transcoding_query_parameters_are_preserved_and_client_id_is_encoded()
    {
        var track = TestTracks.Create();
        track.Media!.Transcodings[0].Url = new Uri(
            "https://api-v2.soundcloud.com/media/test?secret_token=s-test"
        );
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    Assert.Equal(
                        "?secret_token=s-test&client_id=test%26id",
                        request.RequestUri!.Query
                    );
                    return StubHttpMessageHandler.Text(
                        """{"url":"https://audio.example/audio.m3u8"}"""
                    );
                }
            )
        );

        await new SoundCloudClient("test&id", http).Tracks.GetDownloadUrlAsync(track);
    }

    [Fact]
    public async Task Blocked_tracks_fail_before_requesting_media()
    {
        var track = TestTracks.Create();
        track.Policy = "BLOCK";
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (_, _) => throw new InvalidOperationException("Unexpected HTTP call.")
            )
        );

        await Assert.ThrowsAsync<SoundCloudExplode.Exceptions.TrackUnavailableException>(async () =>
            await new SoundCloudClient("test", http).Tracks.GetDownloadUrlAsync(track)
        );
    }

    [Fact]
    public async Task Missing_transcodings_fail_with_a_clear_error()
    {
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (_, _) => throw new InvalidOperationException("Unexpected HTTP call.")
            )
        );

        await Assert.ThrowsAsync<SoundCloudExplode.Exceptions.TrackUnavailableException>(async () =>
            await new SoundCloudClient("test", http).Tracks.GetDownloadUrlAsync(new Track())
        );
    }

    [Theory]
    [InlineData("hls", "video/mp4", true)]
    [InlineData("encrypted_hls", "audio/mp4", true)]
    [InlineData("hls", "audio/mp4", false)]
    public async Task Unsupported_or_missing_media_urls_return_null(
        string protocol,
        string mimeType,
        bool hasUrl
    )
    {
        var track = TestTracks.Create(protocol, mimeType);
        if (!hasUrl)
            track.Media!.Transcodings[0].Url = null;
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (_, _) => throw new InvalidOperationException("Unexpected HTTP call.")
            )
        );

        Assert.Null(await new SoundCloudClient("test", http).Tracks.GetDownloadUrlAsync(track));
    }
}
