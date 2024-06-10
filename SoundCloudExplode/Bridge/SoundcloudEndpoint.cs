using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Bridge;

public class SoundcloudEndpoint(HttpClient http)
{
    public string ClientId { get; set; } = default!;

    public async ValueTask<string> ResolveUrlAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var host = new Uri(url).Host;

        if (host.StartsWith("on.", StringComparison.OrdinalIgnoreCase))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            url = response.RequestMessage!.RequestUri!.ToString();

            var builder = new UriBuilder(url) { Query = "", Fragment = "" };

            url = builder.Uri.ToString();

            host = new Uri(url).Host;
        }

        if (host.StartsWith("m.", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new UriBuilder(url) { Host = host.Substring(2) };

            url = builder.Uri.ToString();
        }

        return await http.ExecuteGetAsync(
            $"{Constants.ResolveEndpoint}?url={url}&client_id={ClientId}",
            cancellationToken
        );
    }
}
