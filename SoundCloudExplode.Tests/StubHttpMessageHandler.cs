using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCloudExplode.Tests;

internal sealed class StubHttpMessageHandler(
    Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> respond
) : HttpMessageHandler
{
    public static HttpResponseMessage Text(string content, string mediaType = "application/json") =>
        new(HttpStatusCode.OK)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, mediaType),
        };

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var response = respond(request, cancellationToken);
        response.RequestMessage ??= request;
        return Task.FromResult(response);
    }
}
