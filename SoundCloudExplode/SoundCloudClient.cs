using System;
using System.Linq;
using Newtonsoft.Json;
using SoundCloudExplode.Track;
using SoundCloudExplode.Playlist;
using SoundCloudExplode.Utils;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using SoundCloudExplode.Exceptions;
using System.Text.RegularExpressions;

namespace SoundCloudExplode
{
    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public class SoundCloudClient
    {
        private string ResolveEndpoint { get; } = "https://api-v2.soundcloud.com/resolve";
        private string TrackEndpoint { get; } = "https://api-v2.soundcloud.com/tracks";
        //private string TrackEndpoint { get; } = "https://api.soundcloud.com/i1/tracks";

        private string ClientId = "a3e059563d7fd3372b49b37f00a00bcf";
        //private string ClientId = "a3dd183a357fcff9a6943c0d65664087";

        private string BaseUrl = "https://soundcloud.com";

        //private readonly Regex PlaylistRegex = new (@"soundcloud\..+?/(.*?)\/sets\/.+");
        //private readonly Regex TrackRegex = new (@"soundcloud\..+?/(.*?)\/sets\/.+");
        private readonly Regex PlaylistRegex = new(@"soundcloud\..+?\/(.*?)\/sets\/[a-zA-Z]+");
        private readonly Regex TrackRegex = new(@"soundcloud\..+?\/(.*?)\/[a-zA-Z0-9~@#$^*()_+=[\]{}|\\,.?: -]+");

        /// <summary>
        /// Initializes an instance of <see cref="SoundCloudClient"/>.
        /// </summary>
        public SoundCloudClient()
        {
            //SetClientId();
        }

        /// <summary>
        /// Initializes an instance of <see cref="SoundCloudClient"/>.
        /// </summary>
        public SoundCloudClient(string clientId)
        {
            ClientId = clientId;
        }

        /// <summary>
        /// Sets Default ClientId
        /// </summary>
        public async void SetClientId(CancellationToken cancellationToken = default)
        {
            var document = new HtmlDocument();
            string html = await Http.GetHtmlAsync(BaseUrl, null, null, cancellationToken);
            document.LoadHtml(html);

            var script = document.DocumentNode.Descendants()
                .Where(x => x.Name == "script").ToList();

            var script_url = script.Last().Attributes["src"].Value;

            html = await Http.GetHtmlAsync(script_url, null, null, cancellationToken);

            ClientId = html.Split(new string[] { ",client_id" }, StringSplitOptions.None)[1].Split('"')[1];
        }

        /// <summary>
        /// Gets track information
        /// </summary>
        public async Task<TrackInformation> GetTrackAsync(
            string? trackUrl,
            CancellationToken cancellationToken = default)
        {
            if (trackUrl is null)
            {
                return new();
            }

            return JsonConvert.DeserializeObject<TrackInformation>(await ResolveSoundcloudUrlAsync(trackUrl, cancellationToken))!;
        }

        /// <summary>
        /// Gets all tracks information
        /// </summary>
        public async Task<List<TrackInformation>> GetTracksAsync(
            string? trackUrl,
            CancellationToken cancellationToken = default)
        {
            var tracks = new List<TrackInformation>();

            if (trackUrl is null)
            {
                return tracks;
            }

            // Only consider URLs when parsing IDs.
            // All other queries are treated as search queries.
            var isUrl = Uri.IsWellFormedUriString(trackUrl, UriKind.Absolute);

            // Playlist
            if (isUrl && PlaylistRegex.IsMatch(trackUrl))
            {
                var playlist = await GetPlaylistAsync(trackUrl, cancellationToken);
                if (playlist is null || playlist.Tracks is null)
                {
                    return tracks;
                }

                foreach (var track in playlist.Tracks)
                {
                    var trackUrl2 = await QueryTrackUrlAsync(track.Id, cancellationToken);
                    var trackInfo = await GetTrackAsync(trackUrl2, cancellationToken);

                    tracks.Add(trackInfo);
                }
            }
            else if (isUrl && TrackRegex.IsMatch(trackUrl))
            {
                tracks.Add(await GetTrackAsync(trackUrl, cancellationToken));
            }

            return tracks;
        }

        /// <summary>
        /// Gets playlist information
        /// </summary>
        public async Task<PlaylistInformation> GetPlaylistAsync(
            string? playlistUrl,
            CancellationToken cancellationToken = default)
        {
            if (playlistUrl is null)
            {
                return new();
            }

            return JsonConvert.DeserializeObject<PlaylistInformation>(await ResolveSoundcloudUrlAsync(playlistUrl, cancellationToken))!;
        }

        private async Task<string> ResolveSoundcloudUrlAsync(
            string soundcloudUrl,
            CancellationToken cancellationToken = default)
        {
            return await Http.GetHtmlAsync($"{ResolveEndpoint}?url={soundcloudUrl}&client_id={ClientId}", null, null, cancellationToken);
        }

        private async Task<string?> QueryTrackMp3Async(
            string trackM3u8,
            CancellationToken cancellationToken = default)
        {
            var html = await Http.GetHtmlAsync(trackM3u8, null, null, cancellationToken);
            var m3u8 = html.Split(',');

            if (m3u8.Length <= 0)
            {
                return null;
            }

            string link = "";

            var last_stream = m3u8.Last().Split('/');
            for (int i = 0; i < last_stream.Length; i++)
            {
                if (last_stream[i] == "media")
                {
                    last_stream[i + 1] = "0";
                    link = string.Join("/", last_stream).Split('\n')[1];
                }
            }

            return link;
        }

        /// <summary>
        /// Gets the download url from a track
        /// </summary>
        public async Task<string?> GetDownloadUrlAsync(
            TrackInformation track,
            CancellationToken cancellationToken = default)
        {
            if (track.Policy?.ToLower() == "block")
            {
                throw new TrackUnavailableException("This track is not available in your country");
            }

            if (track.Media == null
                || track.Media.Transcodings == null
                || track.Media.Transcodings.Length <= 0)
            {
                throw new TrackUnavailableException("No transcodings found");
            }

            var trackUrl = "";

            //progrssive/stream
            var transcoding = track.Media.Transcodings
                .Where(x => x.Quality == "sq"
                && x.Format is not null && x.Format.MimeType is not null
                && x.Format.MimeType.Contains("audio/mpeg") && x.Format.Protocol == "progressive")
                .FirstOrDefault();

            //hls
            if (transcoding == null)
            {
                transcoding = track.Media.Transcodings
                    .Where(x => x.Quality == "sq"
                    && x.Format is not null && x.Format.MimeType is not null
                    && x.Format.MimeType.Contains("ogg") && x.Format.Protocol == "hls")
                   .FirstOrDefault();
            }

            if (transcoding is null || transcoding.Url is null)
            {
                return null;
            }

            trackUrl += transcoding.Url.ToString() + $"?client_id={ClientId}";

            var trackMedia = await Http.GetHtmlAsync(trackUrl, null, null, cancellationToken);
            var track2 = JsonConvert.DeserializeObject<TrackMediaInformation>(trackMedia);

            if (track2 is null)
            {
                return null;
            }

            var trackMediaUrl = track2.Url ?? "";
            if (trackMediaUrl.Contains(".m3u8"))
            {
                return await QueryTrackMp3Async(trackMediaUrl, cancellationToken);
            }

            return trackMediaUrl;
        }

        /// <summary>
        /// Gets track information
        /// </summary>
        public async Task<string?> QueryTrackUrlAsync(
            long trackId,
            CancellationToken cancellationToken = default)
        {
            var trackInfoHtml = await Http.GetHtmlAsync($"{TrackEndpoint}/{trackId}?client_id={ClientId}", null, null, cancellationToken);
            if (trackInfoHtml is null)
            {
                return null;
            }

            var track = JsonConvert.DeserializeObject<TrackInformation>(trackInfoHtml);
            if (track is null || track.PermalinkUrl is null)
            {
                return null;
            }

            return track.PermalinkUrl.ToString();
        }

        /// <summary>
        /// Downloads a track
        /// </summary>
        public async Task DownloadAsync(
            TrackInformation track,
            string filePath,
            IProgress<double>? progress = null,
            CancellationToken cancellationToken = default)
        {
            var mp3TrackMediaUrl = await GetDownloadUrlAsync(track, cancellationToken);
            if (mp3TrackMediaUrl is null)
            {
                return;
            }

            var totalLength = await Http.GetFileSizeAsync(mp3TrackMediaUrl, cancellationToken);

            var downloadRequest = WebRequest.Create(mp3TrackMediaUrl);
            var downloadResponse = downloadRequest.GetResponse();
            var stream = downloadResponse.GetResponseStream();

            var dir = Path.GetDirectoryName(filePath);
            if (dir is null)
            {
                return;
            }

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
}