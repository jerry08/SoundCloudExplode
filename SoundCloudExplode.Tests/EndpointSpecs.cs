using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using Xunit;

namespace SoundCloudExplode.Tests;

public class EndpointSpecs
{
    [Fact]
    public async Task Mobile_urls_and_nested_query_parameters_are_encoded_as_one_resolve_parameter()
    {
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    Assert.Equal("api-v2.soundcloud.com", request.RequestUri!.Host);
                    var query = request.RequestUri.Query.TrimStart('?').Split('&');
                    Assert.Equal(2, query.Length);
                    Assert.Equal(
                        "url=https://soundcloud.com/artist/track?secret_token=s-test&in=artist/sets/mix",
                        Uri.UnescapeDataString(query[0])
                    );
                    Assert.Equal("client_id=test&id", Uri.UnescapeDataString(query[1]));
                    return StubHttpMessageHandler.Text("""{"kind":"track"}""");
                }
            )
        );

        var endpoint = new SoundcloudEndpoint(http) { ClientId = "test&id" };
        await endpoint.ResolveUrlAsync(
            "https://m.soundcloud.com/artist/track?secret_token=s-test&in=artist/sets/mix"
        );
    }

    [Fact]
    public async Task Short_links_preserve_case_and_resolve_the_redirected_mobile_url()
    {
        var requests = 0;
        using var http = new HttpClient(
            new StubHttpMessageHandler(
                (request, _) =>
                {
                    if (++requests == 1)
                    {
                        Assert.Equal(
                            "https://on.soundcloud.com/AbCdE",
                            request.RequestUri!.AbsoluteUri
                        );
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            RequestMessage = new HttpRequestMessage(
                                HttpMethod.Get,
                                "https://m.soundcloud.com/artist/track?utm_source=share#fragment"
                            ),
                        };
                    }

                    Assert.Equal(
                        "?url=https%3A%2F%2Fsoundcloud.com%2Fartist%2Ftrack&client_id=test",
                        request.RequestUri!.Query
                    );
                    return StubHttpMessageHandler.Text("""{"kind":"track"}""");
                }
            )
        );

        await new SoundcloudEndpoint(http) { ClientId = "test" }.ResolveUrlAsync(
            "https://on.soundcloud.com/AbCdE"
        );
        Assert.Equal(2, requests);
    }
}
