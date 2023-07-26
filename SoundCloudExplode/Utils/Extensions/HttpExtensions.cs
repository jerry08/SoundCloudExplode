using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Utils.Extensions;

internal static class HttpExtensions
{
    public static async ValueTask<string> ExecuteGetAsync(
        this HttpClient http,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await http.ExecuteAsync(request, cancellationToken);
    }

    public static async ValueTask<string> ExecuteAsync(
        this HttpClient http,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // User-agent
        if (!request.Headers.Contains("User-Agent"))
        {
            request.Headers.Add(
                "User-Agent",
                Http.ChromeUserAgent()
            );
        }

        using var response = await http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        // Special case check for rate limiting errors
        if ((int)response.StatusCode == 429)
        {
            throw new RequestLimitExceededException(
                "Exceeded request rate limit. " +
                "Please try again later. "
            );
        }

        return !response.IsSuccessStatusCode
            ? throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode})." +
                Environment.NewLine +
                "Request:" +
                Environment.NewLine +
                request
            )
            : await response.Content.ReadAsStringAsync(cancellationToken);
    }
}