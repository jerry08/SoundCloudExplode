using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Bridge;

public class SoundcloudEndpoint
{
    private readonly HttpClient _http;

    public string ClientId { get; set; } = default!;

    public SoundcloudEndpoint(HttpClient http)
    {
        _http = http;
    }

    public async ValueTask<string> ResolveUrlAsync(
        string soundcloudUrl,
        CancellationToken cancellationToken = default)
    {
        var host = new Uri(soundcloudUrl).Host;
        if (host.StartsWith("m."))
        {
            var builder = new UriBuilder(soundcloudUrl)
            {
                Host = host.Substring(2)
            };

            soundcloudUrl = builder.Uri.ToString();
        }

        return await _http.ExecuteGetAsync($"{Constants.ResolveEndpoint}?url={soundcloudUrl}&client_id={ClientId}", cancellationToken);
    }
}