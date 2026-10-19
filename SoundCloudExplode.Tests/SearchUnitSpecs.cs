using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Common;
using SoundCloudExplode.Search;
using Xunit;

namespace SoundCloudExplode.Tests;

public class SearchUnitSpecs
{
    [Fact]
    public async Task Search_deserializes_mixed_results_and_stops_at_an_empty_page()
    {
        var requests = 0;
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    var uri = request.RequestUri!;
                    Assert.Equal("/search", uri.AbsolutePath);
                    Assert.Contains("q=artist%20%26%20song", uri.Query);
                    Assert.Contains("client_id=test-client", uri.Query);
                    if (++requests == 1)
                    {
                        Assert.Contains("offset=0", uri.Query);
                        return StubHttpMessageHandler.Text(
                            """
                            {"collection":[
                                {"kind":"track","id":1,"title":"A track","permalink_url":"https://soundcloud.com/artist/track"},
                                {"kind":"playlist","id":2,"title":"A playlist","permalink_url":"https://soundcloud.com/artist/sets/playlist"},
                                {"kind":"user","id":3,"username":"An artist","permalink_url":"https://soundcloud.com/artist"}
                            ]}
                            """
                        );
                    }
                    Assert.Equal(2, requests);
                    Assert.Contains("offset=3", uri.Query);
                    return StubHttpMessageHandler.Text("""{"collection":[]}""");
                }
            )
        );

        var results = await new SoundCloudClient("test-client", http).Search.GetResultsAsync(
            "artist & song",
            limit: 3
        );

        Assert.Collection(
            results,
            result => Assert.IsType<TrackSearchResult>(result),
            result => Assert.IsType<PlaylistSearchResult>(result),
            result => Assert.IsType<UserSearchResult>(result)
        );
        Assert.Equal(2, requests);
    }

    [Fact]
    public async Task Collecting_a_subset_stops_without_fetching_another_page()
    {
        var requests = 0;
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    Assert.Equal(1, ++requests);
                    Assert.Equal("/search/tracks", request.RequestUri!.AbsolutePath);
                    Assert.Contains("offset=5", request.RequestUri.Query);
                    Assert.Contains("limit=2", request.RequestUri.Query);
                    return StubHttpMessageHandler.Text(
                        """
                        {"collection":[
                            {"kind":"track","id":1,"permalink_url":"https://soundcloud.com/artist/first"},
                            {"kind":"track","id":2,"permalink_url":"https://soundcloud.com/artist/second"}
                        ]}
                        """
                    );
                }
            )
        );

        var results = await new SoundCloudClient("test", http)
            .Search.GetTracksAsync("artist", offset: 5, limit: 2)
            .CollectAsync(1);

        Assert.Single(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public async Task Cancelled_search_does_not_send_a_request()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (_, _) => throw new InvalidOperationException("Unexpected request.")
            )
        );

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new SoundCloudClient("test", http).Search.GetResultsAsync(
                "artist",
                cancellationToken: cancellation.Token
            )
        );
    }
}
