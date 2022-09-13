using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoundCloudExplode.Http;
using SoundCloudExplode.Track;
using SoundCloudExplode.Common;
using SoundCloudExplode.Playlist;
using SoundCloudExplode.Exceptions;

namespace SoundCloudExplode.Tracks;

/// <summary>
/// Operations related to Soundcloud playlist.
/// </summary>
public class PlaylistClient
{
    private readonly SoundCloudClient _client;

    private readonly Regex PlaylistRegex = new(@"soundcloud\..+?\/(.*?)\/sets\/[a-zA-Z]+");

    /// <summary>
    /// Initializes an instance of <see cref="PlaylistClient"/>.
    /// </summary>
    public PlaylistClient(SoundCloudClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Checks for valid playlist url
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    /// <exception cref="SoundcloudExplodeException"></exception>
    public bool IsUrlValid(string url)
    {
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        if (isUrl && PlaylistRegex.IsMatch(url))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the metadata associated with the specified playlist.
    /// </summary>
    public async ValueTask<PlaylistInformation?> GetAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var resolvedJson = await _client.ResolveSoundcloudUrlAsync
            (url, cancellationToken);

        return JsonConvert.DeserializeObject<PlaylistInformation>(resolvedJson);
    }

    /// <summary>
    /// Enumerates batches of tracks included in the specified playlist.
    /// </summary>
    public async IAsyncEnumerable<Batch<TrackInformation>> GetTrackBatchesAsync(
        string url,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var playlist = await GetAsync(url, cancellationToken);
        if (playlist is null || playlist.Tracks is null)
            yield break;

        var tracks = new List<TrackInformation>();

        foreach (var track in playlist.Tracks)
        {
            var trackInfo = await _client.Tracks.GetByIdAsync(track.Id, cancellationToken);
            if (trackInfo is null)
                continue;

            tracks.Add(trackInfo);

            yield return Batch.Create(tracks);
        }
    }

    /// <summary>
    /// Enumerates tracks included in the specified playlist url.
    /// </summary>
    public IAsyncEnumerable<TrackInformation> GetTracksAsync(
        string url,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, cancellationToken).FlattenAsync();
}