using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Exceptions;
using Xunit;

namespace SoundCloudExplode.Tests;

public sealed class DownloadSpecs : IDisposable
{
    private readonly string _path = Path.Combine(
        Path.GetTempPath(),
        $"SoundCloudExplode-{Guid.NewGuid():N}.audio"
    );

    [Fact]
    public async Task Aac_hls_download_includes_initialization_and_all_segments_in_order()
    {
        var requests = new List<string>();
        using var http = CreateHttp(request =>
        {
            Assert.Equal("test-value", Assert.Single(request.Headers.GetValues("X-Test")));
            var path = request.RequestUri!.AbsolutePath;
            requests.Add(path);
            return path switch
            {
                "/stream" => StubHttpMessageHandler.Text(
                    """
                    #EXTM3U
                    #EXT-X-MAP:URI="init.mp4?signature=a%2Bb"
                    #EXTINF:10,
                    data000.m4s
                    #EXTINF:10,
                    /data001.m4s
                    #EXT-X-ENDLIST
                    """,
                    "application/octet-stream"
                ),
                "/init.mp4" => Initialization(request),
                "/data000.m4s" => StubHttpMessageHandler.Text("FIRST", "audio/mp4"),
                "/data001.m4s" => StubHttpMessageHandler.Text("SECOND", "audio/mp4"),
                _ => throw new InvalidOperationException(path),
            };
        });
        var progress = new RecordedProgress();

        await new SoundCloudClient("test", http).DownloadAsync(
            TestTracks.Create(),
            _path,
            progress,
            new Dictionary<string, string> { ["X-Test"] = "test-value" }
        );

        Assert.Equal("INITFIRSTSECOND", await File.ReadAllTextAsync(_path));
        Assert.Equal(new[] { "/stream", "/init.mp4", "/data000.m4s", "/data001.m4s" }, requests);
        Assert.Equal(1, progress.Values[^1]);
        Assert.All(progress.Values, value => Assert.InRange(value, 0, 1));

        static HttpResponseMessage Initialization(HttpRequestMessage request)
        {
            Assert.Equal("?signature=a%2Bb", request.RequestUri!.Query);
            return StubHttpMessageHandler.Text("INIT", "audio/mp4");
        }
    }

    [Fact]
    public async Task Legacy_mp3_hls_downloads_every_segment_without_rewriting_urls()
    {
        using var http = CreateHttp(request =>
            request.RequestUri!.AbsolutePath switch
            {
                "/stream" => StubHttpMessageHandler.Text(
                    "#EXTM3U\r\n#EXTINF:1,\r\nmedia/0/10.mp3\r\n#EXTINF:1,\r\nmedia/10/20.mp3\r\n#EXT-X-ENDLIST\r\n"
                ),
                "/media/0/10.mp3" => StubHttpMessageHandler.Text("FIRST", "audio/mpeg"),
                "/media/10/20.mp3" => StubHttpMessageHandler.Text("SECOND", "audio/mpeg"),
                _ => throw new InvalidOperationException("Unexpected URL."),
            }
        );

        await new SoundCloudClient("test", http).DownloadAsync(
            TestTracks.Create(mimeType: "audio/mpeg", quality: "sq"),
            _path
        );

        Assert.Equal("FIRSTSECOND", await File.ReadAllTextAsync(_path));
    }

    [Fact]
    public async Task Adaptive_hls_uses_the_highest_bandwidth_variant_and_resolves_relative_urls()
    {
        using var http = CreateHttp(request =>
            request.RequestUri!.AbsolutePath switch
            {
                "/stream" => StubHttpMessageHandler.Text(
                    """
                    #EXTM3U
                    #EXT-X-STREAM-INF:BANDWIDTH=64000
                    low/playlist.m3u8
                    #EXT-X-STREAM-INF:BANDWIDTH=128000
                    high/playlist.m3u8
                    """
                ),
                "/high/playlist.m3u8" => StubHttpMessageHandler.Text(
                    "#EXTM3U\n#EXTINF:1,\naudio.mp3\n#EXT-X-ENDLIST"
                ),
                "/high/audio.mp3" => StubHttpMessageHandler.Text("HIGH", "audio/mpeg"),
                _ => throw new InvalidOperationException("Unexpected URL."),
            }
        );

        await new SoundCloudClient("test", http).DownloadAsync(
            TestTracks.Create(mimeType: "audio/mpegurl"),
            _path
        );

        Assert.Equal("HIGH", await File.ReadAllTextAsync(_path));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Progressive_downloads_copy_audio_and_report_finite_progress(
        bool unknownLength
    )
    {
        using var http = CreateHttp(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = unknownLength ? new UnknownLengthContent() : new StringContent("AUDIO"),
        });
        var progress = new RecordedProgress();

        await new SoundCloudClient("test", http).DownloadAsync(
            TestTracks.Create("progressive", "audio/mpeg", "sq"),
            _path,
            progress
        );

        Assert.Equal("AUDIO", await File.ReadAllTextAsync(_path));
        Assert.Equal(1, progress.Values[^1]);
        Assert.All(
            progress.Values,
            value => Assert.True(double.IsFinite(value) && value is >= 0 and <= 1)
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("not a playlist")]
    [InlineData("#EXTM3U\n#EXT-X-ENDLIST")]
    [InlineData("#EXTM3U\n#EXTINF:1,\naudio.m4s")]
    [InlineData(
        "#EXTM3U\n#EXT-X-KEY:METHOD=AES-128,URI=\"key\"\n#EXTINF:1,\naudio.m4s\n#EXT-X-ENDLIST"
    )]
    [InlineData("#EXTM3U\n#EXT-X-BYTERANGE:20@0\n#EXTINF:1,\naudio.m4s\n#EXT-X-ENDLIST")]
    [InlineData(
        "#EXTM3U\n#EXT-X-MAP:URI=\"init.mp4\",BYTERANGE=\"20@0\"\n#EXTINF:1,\naudio.m4s\n#EXT-X-ENDLIST"
    )]
    [InlineData("#EXTM3U\n#EXT-X-DISCONTINUITY\n#EXTINF:1,\naudio.m4s\n#EXT-X-ENDLIST")]
    [InlineData("#EXTM3U\n#EXTINF:1,\nfile:///audio.mp3\n#EXT-X-ENDLIST")]
    public async Task Unsupported_or_invalid_playlists_fail_before_creating_a_file(string playlist)
    {
        using var http = CreateHttp(_ => StubHttpMessageHandler.Text(playlist));

        await Assert.ThrowsAsync<SoundcloudExplodeException>(async () =>
            await new SoundCloudClient("test", http).DownloadAsync(TestTracks.Create(), _path)
        );

        Assert.False(File.Exists(_path));
    }

    [Fact]
    public async Task Cyclic_master_playlists_stop_without_looping()
    {
        var requests = 0;
        using var http = CreateHttp(_ =>
        {
            Assert.True(++requests <= 2);
            return StubHttpMessageHandler.Text(
                "#EXTM3U\n#EXT-X-STREAM-INF:BANDWIDTH=128000\n/stream"
            );
        });

        await Assert.ThrowsAsync<SoundcloudExplodeException>(async () =>
            await new SoundCloudClient("test", http).DownloadAsync(TestTracks.Create(), _path)
        );
        Assert.False(File.Exists(_path));
    }

    [Fact]
    public async Task Cancellation_stops_before_requesting_the_next_segment()
    {
        using var cancellation = new CancellationTokenSource();
        using var http = CreateHttp(request =>
            request.RequestUri!.AbsolutePath switch
            {
                "/stream" => StubHttpMessageHandler.Text(
                    "#EXTM3U\n#EXTINF:1,\nfirst.mp3\n#EXTINF:1,\nsecond.mp3\n#EXT-X-ENDLIST"
                ),
                "/first.mp3" => StubHttpMessageHandler.Text("FIRST"),
                _ => throw new InvalidOperationException(
                    "The second segment should not be requested."
                ),
            }
        );
        var progress = new RecordedProgress(_ => cancellation.Cancel());

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new SoundCloudClient("test", http).DownloadAsync(
                TestTracks.Create(),
                _path,
                progress,
                cancellationToken: cancellation.Token
            )
        );
    }

    [Fact]
    public async Task Failed_media_requests_do_not_create_a_successful_download()
    {
        using var http = CreateHttp(_ => new HttpResponseMessage(HttpStatusCode.NotFound));

        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await new SoundCloudClient("test", http).DownloadAsync(TestTracks.Create(), _path)
        );
        Assert.False(File.Exists(_path));
    }

    [Fact]
    public async Task An_unavailable_stream_does_not_silently_report_success()
    {
        using var http = CreateHttp(_ =>
            throw new InvalidOperationException("Unexpected request.")
        );
        var track = TestTracks.Create(mimeType: "video/mp4");

        await Assert.ThrowsAsync<TrackUnavailableException>(async () =>
            await new SoundCloudClient("test", http).DownloadAsync(track, _path)
        );
        Assert.False(File.Exists(_path));
    }

    private static HttpClient CreateHttp(Func<HttpRequestMessage, HttpResponseMessage> respond) =>
        new(
            new StubHttpMessageHandler(
                (request, _) =>
                    request.RequestUri!.Host == "api-v2.soundcloud.com"
                        ? StubHttpMessageHandler.Text("""{"url":"https://audio.example/stream"}""")
                        : respond(request)
            )
        );

    public void Dispose() => File.Delete(_path);

    private sealed class RecordedProgress(Action<double>? onReport = null) : IProgress<double>
    {
        public List<double> Values { get; } = [];

        public void Report(double value)
        {
            Values.Add(value);
            onReport?.Invoke(value);
        }
    }

    private sealed class UnknownLengthContent : HttpContent
    {
        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
            stream.WriteAsync(Encoding.UTF8.GetBytes("AUDIO"), 0, 5);

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
