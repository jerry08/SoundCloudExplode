using System;
using System.Linq;
using Newtonsoft.Json;
using SoundCloudExplode.Models.SoundCloud;
using SoundCloudExplode.Models.Playlist;
using SoundCloudExplode.Utils;

namespace SoundCloudExplode
{
    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudClient"/>.
    /// </summary>
    public class SoundCloudClient
    {
        private string ResolveEndpoint { get; } = "https://api-v2.SoundCloud.com/resolve";
        private string TrackEndpoint { get; } = "https://api-v2.SoundCloud.com/tracks";

        /// <summary>
        /// Gets track information
        /// </summary>
        public TrackInformation ResolveTrackUrl(string trackUrl, string clientId)
        {
            return JsonConvert.DeserializeObject<TrackInformation>(ResolveSoundcloudUrl(trackUrl, clientId));
        }

        /// <summary>
        /// Gets playlist information
        /// </summary>
        public PlaylistInformation ResolvePlaylistUrl(string playlistUrl, string clientId)
        {
            return JsonConvert.DeserializeObject<PlaylistInformation>(ResolveSoundcloudUrl(playlistUrl, clientId));
        }

        private string ResolveSoundcloudUrl(string soundcloudUrl, string clientId)
        {
            return Http.GetHtml($"{ResolveEndpoint}?url={soundcloudUrl}&client_id={clientId}");
        }

        /// <summary>
        /// Select at least one working transcoding
        /// </summary>
        public Uri QueryTrackTranscodings(Models.SoundCloud.Transcoding[] trackTranscodings, string clientId)
        {
            var trackUrl = trackTranscodings
                .Where(x => x.Quality == "sq" && x.Format.MimeType.Contains("ogg") && x.Format.Protocol == "hls").FirstOrDefault()
                .Url;
            return new Uri(trackUrl + $"?client_id={clientId}");
        }

        public Uri QueryTrackM3u8(string trackTranscoding)
        {
            var trackMedia = Http.GetHtml(trackTranscoding);
            return new Uri(JsonConvert.DeserializeObject<TrackMediaInformation>(trackMedia).Url);
        }

        public Uri QueryTrackMp3(string trackM3u8)
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

            return new Uri(link);
        }

        /// <summary>
        /// Gets track information
        /// </summary>
        public Uri QueryTrackUrl(long trackId, string clientId)
        {
            var trackInformation = Http.GetHtml($"{TrackEndpoint}/{trackId}?client_id={clientId}");
            return JsonConvert.DeserializeObject<TrackInformation>(trackInformation).PermalinkUrl;
        }
    }
}