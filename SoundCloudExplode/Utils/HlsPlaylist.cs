using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Utils;

internal static class HlsPlaylist
{
    public static bool IsPlaylist(Uri url, HttpContent content) =>
        url.AbsolutePath.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase)
        || content.Headers.ContentType?.MediaType?.ToLowerInvariant()
            is "application/vnd.apple.mpegurl"
                or "application/x-mpegurl"
                or "audio/mpegurl"
                or "audio/x-mpegurl";

    public static async Task<IReadOnlyList<Uri>> GetSegmentsAsync(
        Uri playlistUrl,
        string playlist,
        Func<Uri, Task<string>> loadPlaylist
    )
    {
        var visited = new HashSet<Uri>();

        // Bound master-playlist traversal, including malformed/cyclic responses.
        while (visited.Count < 5 && visited.Add(playlistUrl))
        {
            var lines = playlist.Split('\n').Select(x => x.Trim()).ToArray();
            if (lines.FirstOrDefault() != "#EXTM3U")
                throw new SoundcloudExplodeException("Invalid HLS playlist.");

            var segments = new List<Uri>();
            var variants = new List<(Uri Url, long Bandwidth)>();
            long? variantBandwidth = null;
            Uri? initializationUrl = null;
            var mediaSegmentCount = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("#EXT-X-KEY:", StringComparison.Ordinal))
                {
                    if (!Regex.IsMatch(line, @"(?:[:,])METHOD=NONE(?:,|$)"))
                        throw new SoundcloudExplodeException(
                            "Encrypted HLS downloads are not supported."
                        );
                }
                else if (
                    line.StartsWith("#EXT-X-BYTERANGE:", StringComparison.Ordinal)
                    || line.StartsWith("#EXT-X-DISCONTINUITY", StringComparison.Ordinal)
                    || line.StartsWith("#EXT-X-GAP", StringComparison.Ordinal)
                )
                {
                    throw new SoundcloudExplodeException(
                        "HLS byte ranges, gaps and discontinuities are not supported."
                    );
                }
                else if (line.StartsWith("#EXT-X-MAP:", StringComparison.Ordinal))
                {
                    if (line.Contains("BYTERANGE="))
                        throw new SoundcloudExplodeException("HLS byte ranges are not supported.");

                    var match = Regex.Match(line, "(?:[:,])URI=\"([^\"]+)\"");
                    if (!match.Success)
                        throw new SoundcloudExplodeException(
                            "The HLS initialization segment is missing its URL."
                        );

                    var url = ResolveUrl(playlistUrl, match.Groups[1].Value);
                    if (initializationUrl != url)
                    {
                        if (mediaSegmentCount > 0)
                            throw new SoundcloudExplodeException(
                                "Changing HLS initialization segments is not supported."
                            );

                        segments.Add(url);
                        initializationUrl = url;
                    }
                }
                else if (line.StartsWith("#EXT-X-STREAM-INF:", StringComparison.Ordinal))
                {
                    var match = Regex.Match(line, @"(?:[:,])BANDWIDTH=(\d+)");
                    variantBandwidth = long.TryParse(match.Groups[1].Value, out var bandwidth)
                        ? bandwidth
                        : 0;
                }
                else if (line.Length > 0 && !line.StartsWith("#", StringComparison.Ordinal))
                {
                    var url = ResolveUrl(playlistUrl, line);
                    if (variantBandwidth is { } bandwidth)
                    {
                        variants.Add((url, bandwidth));
                        variantBandwidth = null;
                    }
                    else
                    {
                        segments.Add(url);
                        mediaSegmentCount++;
                    }
                }
            }

            if (variants.Count > 0)
            {
                playlistUrl = variants.OrderByDescending(x => x.Bandwidth).First().Url;
                playlist = await loadPlaylist(playlistUrl);
                continue;
            }

            if (mediaSegmentCount == 0 || !lines.Contains("#EXT-X-ENDLIST"))
                throw new SoundcloudExplodeException(
                    "The HLS playlist is empty or is not a complete on-demand stream."
                );

            return segments;
        }

        throw new SoundcloudExplodeException(
            "The HLS playlist contains too many nested or cyclic playlists."
        );
    }

    private static Uri ResolveUrl(Uri playlistUrl, string value)
    {
        if (
            !Uri.TryCreate(playlistUrl, value, out var url) || url.Scheme is not ("http" or "https")
        )
            throw new SoundcloudExplodeException("The HLS playlist contains an invalid media URL.");

        return url;
    }
}
