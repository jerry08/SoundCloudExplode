using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using SoundCloudExplode.Track;
using SoundCloudExplode.Common;
using SoundCloudExplode.Playlist;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Utils;
using SoundCloudExplode.Utils.Extensions;

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
        url = url.ToLower();
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        return isUrl && PlaylistRegex.IsMatch(url);
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
        int maxConcurrent = 1,
        int maxChunks = 1,
        int offset = 0,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsUrlValid(url))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var playlist = await GetAsync(url, cancellationToken);
        if (playlist is null || playlist.Tracks is null)
            yield break;

        if (maxChunks < maxConcurrent)
            maxChunks = maxConcurrent;

        var encounteredIds = new List<long>();

        var list = playlist.Tracks.Skip(offset).ToList();
        var chunks = list.ChunkBy(maxChunks);

        var semaphore = new ResizableSemaphore();
        semaphore.MaxCount = maxConcurrent;

        foreach (var chunk in chunks)
        {
            var tasks = chunk.Select(track => Task.Run(async () =>
            {
                using var access = await semaphore.AcquireAsync();

                // Don't yield the same track twice
                if (!encounteredIds.Contains(track.Id))
                    encounteredIds.Add(track.Id);
                else
                    return null;

                var trackInfo = await _client.Tracks.GetByIdAsync(track.Id, cancellationToken);
                if (trackInfo is null)
                    return null;

                return trackInfo;
            }));

            var tracks = await Task.WhenAll(tasks);
            foreach (var trackInfo in tracks)
            {
                if (trackInfo is null)
                    continue;

                yield return Batch.Create(new[] { trackInfo });
            }
        }
    }

    /// <summary>
    /// Enumerates tracks included in the specified playlist url.
    /// </summary>
    public IAsyncEnumerable<TrackInformation> GetTracksAsync(
        string url,
        int maxConcurrent = 1,
        int maxChunks = 1,
        int offset = 0,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, maxConcurrent, maxChunks, offset, cancellationToken).FlattenAsync();
}