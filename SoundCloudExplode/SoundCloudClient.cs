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

        private async void SetClientId()
        {
            var document = new HtmlDocument();
            string html = await Http.GetHtmlAsync(BaseUrl);
            document.LoadHtml(html);

            var script = document.DocumentNode.Descendants()
                .Where(x => x.Name == "script").ToList();

            var script_url = script.Last().Attributes["src"].Value;

            html = await Http.GetHtmlAsync(script_url);

            ClientId = html.Split(new string[] { ",client_id" }, StringSplitOptions.None)[1].Split('"')[1];
        }

        /// <summary>
        /// Gets track information
        /// </summary>
        public async Task<TrackInformation> GetTrackAsync(string? trackUrl)
        {
            if (trackUrl is null)
                return new();

            return JsonConvert.DeserializeObject<TrackInformation>(await ResolveSoundcloudUrl(trackUrl))!;
        }

        /// <summary>
        /// Gets all tracks information
        /// </summary>
        public async Task<List<TrackInformation>> GetTracksAsync(string? trackUrl)
        {
            var tracks = new List<TrackInformation>();

            if (trackUrl is null)
                return tracks;

            if (trackUrl.Contains("/search"))
                return tracks;

            //Paylist
            if (trackUrl.Contains("/sets/"))
            {
                var playlist = await GetPlaylistAsync(trackUrl);
                foreach (var track in playlist.Tracks)
                {
                    var trackUrl2 = await QueryTrackUrl(track.Id);
                    var trackInfo = await GetTrackAsync(trackUrl2);

                    tracks.Add(trackInfo);
                }
            }
            else
            {
                tracks.Add(await GetTrackAsync(trackUrl));
            }

            return tracks;
        }

        /// <summary>
        /// Gets playlist information
        /// </summary>
        public async Task<PlaylistInformation> GetPlaylistAsync(string? playlistUrl)
        {
            if (playlistUrl is null)
                return new();

            return JsonConvert.DeserializeObject<PlaylistInformation>(await ResolveSoundcloudUrl(playlistUrl))!;
        }

        private async Task<string> ResolveSoundcloudUrl(string soundcloudUrl)
        {
            return await Http.GetHtmlAsync($"{ResolveEndpoint}?url={soundcloudUrl}&client_id={ClientId}");
        }

        private async Task<string?> QueryTrackMp3(string trackM3u8)
        {
            var html = await Http.GetHtmlAsync(trackM3u8);
            var m3u8 = html.Split(',');

            if (m3u8.Length <= 0)
                return null;

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
        public async Task<string?> GetDownloadUrl(TrackInformation track)
        {
            var trackUrl = "";

            //progrssive/stream
            var transcoding = track.Media.Transcodings
                .Where(x => x.Quality == "sq" && x.Format.MimeType.Contains("audio/mpeg") && x.Format.Protocol == "progressive")
                .FirstOrDefault();
            
            //hls
            if (transcoding == null)
            {
                transcoding = track.Media.Transcodings
                    .Where(x => x.Quality == "sq" && x.Format.MimeType.Contains("ogg") && x.Format.Protocol == "hls")
                    .FirstOrDefault();
            }

            if (transcoding is null)
                return null;

            trackUrl += transcoding.Url.ToString() + $"?client_id={ClientId}";

            var trackMedia = await Http.GetHtmlAsync(trackUrl);
            var track2 = JsonConvert.DeserializeObject<TrackMediaInformation>(trackMedia);
            
            if (track2 is null)
                return null;

            var trackMediaUrl = track2.Url;

            if (trackMediaUrl.Contains(".m3u8"))
            {
                return await QueryTrackMp3(trackMediaUrl);
            }
            
            return trackMediaUrl;
        }

        /// <summary>
        /// Gets track information
        /// </summary>
        public async Task<string?> QueryTrackUrl(long trackId)
        {
            var trackInformation = await Http.GetHtmlAsync($"{TrackEndpoint}/{trackId}?client_id={ClientId}");
            var track = JsonConvert.DeserializeObject<TrackInformation>(trackInformation);
            
            if (track is null)
                return null;

            return track.PermalinkUrl.ToString();
        }

        /// <summary>
        /// Downloads a track
        /// </summary>
        public async Task Download(TrackInformation track, string filePath)
        {
            var mp3TrackMediaUrl = await GetDownloadUrl(track);

            if (mp3TrackMediaUrl is null)
                return;

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