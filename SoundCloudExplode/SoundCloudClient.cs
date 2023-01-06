using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SoundCloudExplode.Track;
using SoundCloudExplode.Utils.Extensions;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Utils;

namespace SoundCloudExplode;

/// <summary>
/// Initializes an instance of <see cref="SoundCloudClient"/>.
/// </summary>
public class SoundCloudClient
{
    internal string ClientId { get; private set; }

    //private readonly Regex PlaylistRegex = new (@"soundcloud\..+?/(.*?)\/sets\/.+");
    //private readonly Regex TrackRegex = new (@"soundcloud\..+?/(.*?)\/sets\/.+");
    private readonly Regex PlaylistRegex = new(@"soundcloud\..+?\/(.*?)\/sets\/[a-zA-Z]+");
    private readonly Regex TrackRegex = new(@"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+");

    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    private readonly string BaseUrl = "https://soundcloud.com";

    /// <summary>
    /// Operations related to Soundcloud tracks.
    /// </summary>
    public TrackClient Tracks { get; }

    /// <summary>
    /// Operations related to Soundcloud playlists.
    /// </summary>
    public PlaylistClient Playlists { get; }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(string clientId, HttpClient http)
    {
        ClientId = clientId;
        _http = http;
        _endpoint = new(http);
        _endpoint.ClientId = clientId;

        Tracks = new TrackClient(http, _endpoint);
        Playlists = new PlaylistClient(http, _endpoint, Tracks);
    }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient() : this(Constants.ClientId, Http.Client)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(HttpClient http) : this(Constants.ClientId, http)
    {
    }

    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public SoundCloudClient(string clientId) : this(Http.Client)
    {
        ClientId = clientId;
    }

    /// <summary>
    /// Sets Default ClientId
    /// </summary>
    public async Task SetClientIdAsync(CancellationToken cancellationToken = default)
    {
        ClientId = await GetClientIdAsync(cancellationToken);
        _endpoint.ClientId = ClientId;
    }

    /// <summary>
    /// Gets ClientId
    /// </summary>
    public async Task<string> GetClientIdAsync(CancellationToken cancellationToken = default)
    {
        var html = await _http.ExecuteGetAsync(BaseUrl, cancellationToken);
        var document = new HtmlDocument();
        document.LoadHtml(html);

        var script = document.DocumentNode.Descendants()
            .Where(x => x.Name == "script").ToList();

        var script_url = script.Last().Attributes["src"].Value;

        html = await _http.ExecuteGetAsync(script_url, cancellationToken);

        return html.Split(new string[] { ",client_id" }, StringSplitOptions.None)[1].Split('"')[1];
    }

    /// <summary>
    /// Downloads a track
    /// </summary>
    public async ValueTask DownloadAsync(
        TrackInformation track,
        string filePath,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var mp3TrackMediaUrl = await Tracks.GetDownloadUrlAsync(track, cancellationToken);
        if (mp3TrackMediaUrl is null)
            return;

        var totalLength = await _http.GetFileSizeAsync(mp3TrackMediaUrl, cancellationToken);

        var downloadRequest = WebRequest.Create(mp3TrackMediaUrl);
        var downloadResponse = downloadRequest.GetResponse();
        var stream = downloadResponse.GetResponseStream();

        var dir = Path.GetDirectoryName(filePath);
        if (dir is null)
            return;

        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        //Create a stream for the file
        var file = File.Create(filePath);

        try
        {
            double totProgress = 0;

            //This controls how many bytes to read at a time and send to the client
            int bytesToRead = 10000;

            // Buffer to read bytes in chunk size specified above
            byte[] buffer = new byte[bytesToRead];

            int length;
            do
            {
                // Read data into the buffer.
                length = stream.Read(buffer, 0, bytesToRead);

                // and write it out to the response's output stream
                file.Write(buffer, 0, length);

                // Flush the data
                stream.Flush();

                //Clear the buffer
                buffer = new byte[bytesToRead];

                totProgress = (double)file.Length / (double)totalLength * 100;

                progress?.Report(totProgress / 100);
            } while (length > 0); //Repeat until no data is read
        }
        finally
        {
            file?.Close();
            stream?.Close();
        }
    }
}