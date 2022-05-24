using System;
using System.Linq;
using Newtonsoft.Json;
using SoundCloudExplode.Track;
using SoundCloudExplode.Playlist;
using SoundCloudExplode.Utils;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace SoundCloudExplode
{
    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public class SoundCloudClient
    {
        private string ResolveEndpoint { get; } = "https://api-v2.SoundCloud.com/resolve";
        private string TrackEndpoint { get; } = "https://api-v2.SoundCloud.com/tracks";

        private string ClientId;

        private string BaseUrl = "https://SoundCloud.com";

        /// <summary>
        /// Initializes an instance of <see cref="SoundCloudClient"/>.
        /// </summary>
        public SoundCloudClient()
        {
            SetClientId();
        }

        private async void SetClientId()
        {
            var document = new HtmlDocument();
            string html = await Http.GetHtmlAsync(BaseUrl);
            document.LoadHtml(html);

            var tt = document.DocumentNode.Descendants()
                .Where(x => x.Name == "script").ToList();

            var script_url = tt.LastOrDefault().Attributes["src"].Value;

            html = await Http.GetHtmlAsync(script_url);

            ClientId = html.Split(new string[] { ",client_id" }, StringSplitOptions.None)[1].Split('"')[1];
        }


        /// <summary>
        /// Gets track information
        /// </summary>
        public TrackInformation ResolveTrackUrl(string trackUrl)
        {
            return JsonConvert.DeserializeObject<TrackInformation>(ResolveSoundcloudUrl(trackUrl));
        }

        /// <summary>
        /// Gets playlist information
        /// </summary>
        public PlaylistInformation ResolvePlaylistUrl(string playlistUrl)
        {
            return JsonConvert.DeserializeObject<PlaylistInformation>(ResolveSoundcloudUrl(playlistUrl));
        }

        private string ResolveSoundcloudUrl(string soundcloudUrl)
        {
            return Http.GetHtml($"{ResolveEndpoint}?url={soundcloudUrl}&client_id={ClientId}");
        }

        /// <summary>
        /// Select at least one working transcoding
        /// </summary>
        public string QueryTrackTranscodings(Track.Transcoding[] trackTranscodings)
        {
            var trackUrl = trackTranscodings
                .Where(x => x.Quality == "sq" && x.Format.MimeType.Contains("ogg") && x.Format.Protocol == "hls").FirstOrDefault()
                .Url;
            return trackUrl + $"?client_id={ClientId}";
        }

        public string QueryTrackM3u8(string trackTranscoding)
        {
            var trackMedia = Http.GetHtml(trackTranscoding);
            return JsonConvert.DeserializeObject<TrackMediaInformation>(trackMedia).Url;
        }

        public string QueryTrackMp3(string trackM3u8)
        {
            var html = Http.GetHtml(trackM3u8);
            var m3u8 = html.Split(',');

            string link = "";

            var last_stream = m3u8.LastOrDefault().Split('/');
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
        /// Gets track information
        /// </summary>
        public string QueryTrackUrl(long trackId)
        {
            var trackInformation = Http.GetHtml($"{TrackEndpoint}/{trackId}?client_id={ClientId}");
            return JsonConvert.DeserializeObject<TrackInformation>(trackInformation).PermalinkUrl.ToString();
        }

        public async void DownloadAsync(TrackInformation track, string filePath)
        {
            var trackMediaInformation = QueryTrackTranscodings(track.Media.Transcodings);
            var trackMediaUrl = QueryTrackM3u8(trackMediaInformation);
            var mp3TrackMediaUrl = QueryTrackMp3(trackMediaUrl);

            var downloadRequest = (HttpWebRequest)WebRequest.Create(mp3TrackMediaUrl);
            var downloadResponse = (HttpWebResponse)await downloadRequest.GetResponseAsync();
            var stream = downloadResponse.GetResponseStream();

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
                    length = await stream.ReadAsync(buffer, 0, bytesToRead);

                    // and write it out to the response's output stream
                    await file.WriteAsync(buffer, 0, length);

                    // Flush the data
                    await stream.FlushAsync();

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