using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCloudExplode.Http;

internal class NetHttpClient
{
    private readonly HttpClient _http;

    public NetHttpClient(HttpClient http)
    {
        _http = http;
    }

    public async ValueTask<string> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await SendHttpRequestAsync(request, cancellationToken);
    }

    public async ValueTask<string> SendHttpRequestAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // User-agent
        if (!request.Headers.Contains("User-Agent"))
        {
            //request.Headers.Add(
            //    "User-Agent",
            //    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
            //);

            request.Headers.Add(
                "User-Agent",
                Utils.Http.ChromeUserAgent()
            );
        }

        // Set required cookies
        //request.Headers.Add("Cookie", "CONSENT=YES+cb; YSC=DwKYllHNwuw");

        using var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        // Special case check for rate limiting errors
        //if ((int)response.StatusCode == 429)
        //{
        //    throw new RequestLimitExceededException(
        //        "Exceeded request rate limit. " +
        //        "Please try again in a few hours. " +
        //        "Alternatively, inject an instance of HttpClient that includes cookies for authenticated user."
        //    );
        //}

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

    public async ValueTask<long?> GetFileSizeAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        return await GetFileSizeAsync(request, cancellationToken);
    }

    public async ValueTask<long?> GetFileSizeAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        // User-agent
        if (!request.Headers.Contains("User-Agent"))
        {
            //request.Headers.Add(
            //    "User-Agent",
            //    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.114 Safari/537.36"
            //);

            request.Headers.Add(
                "User-Agent",
                Utils.Http.ChromeUserAgent()
            );
        }

        // Set required cookies
        //request.Headers.Add("Cookie", "CONSENT=YES+cb; YSC=DwKYllHNwuw");

        using var response = await _http.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken
        );

        return !response.IsSuccessStatusCode
            ? throw new HttpRequestException(
                $"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode})." +
                Environment.NewLine +
                "Request:" +
                Environment.NewLine +
                request
            )
            : response.Content.Headers.ContentLength;
    }
}