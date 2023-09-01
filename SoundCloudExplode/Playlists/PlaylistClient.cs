using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Bridge;
using SoundCloudExplode.Common;
using SoundCloudExplode.Exceptions;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Utils.Extensions;

namespace SoundCloudExplode.Playlists;

/// <summary>
/// Operations related to Soundcloud playlist/album.
/// (Note: Everything for Playlists and Albums are handled the same.)
/// </summary>
public class PlaylistClient
{
    private readonly HttpClient _http;
    private readonly SoundcloudEndpoint _endpoint;

    private readonly Regex ShortUrlRegex = new(@"on\.soundcloud\..+?\/.+?");
    private readonly Regex PlaylistRegex = new(@"soundcloud\..+?\/(.*?)\/sets\/[a-zA-Z]+");

    /// <summary>
    /// Initializes an instance of <see cref="PlaylistClient"/>.
    /// </summary>
    public PlaylistClient(
        HttpClient http,
        SoundcloudEndpoint endpoint)
    {
        _http = http;
        _endpoint = endpoint;
    }

    /// <summary>
    /// Checks for valid playlist url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async Task<bool> IsUrlValidAsync(
        string url,
        CancellationToken cancellationToken = default)
    {
        if (ShortUrlRegex.IsMatch(url))
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            using var response = await _http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );

            url = response.RequestMessage!.RequestUri!.ToString();
        }

        url = url.ToLower();
        var isUrl = Uri.IsWellFormedUriString(url, UriKind.Absolute);
        return isUrl && PlaylistRegex.IsMatch(url);
    }

    /// <summary>
    /// Gets the metadata associated with the specified playlist.
    /// </summary>
    /// <param name="url"></param>
    /// <param name="populateAllTracks">Set to true if you want to populate all tracks in playlist.
    /// information at the same time. If false, only the tracks id and playlist info will return.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <exception cref="SoundcloudExplodeException"/>
    public async ValueTask<Playlist> GetAsync(
        string url,
        bool populateAllTracks = false,
        CancellationToken cancellationToken = default)
    {
        if (!await IsUrlValidAsync(url, cancellationToken))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var resolvedJson = await _endpoint.ResolveUrlAsync(url, cancellationToken);
        var playlist = JsonSerializer.Deserialize<Playlist>(resolvedJson)!;

        if (populateAllTracks)
        {
            var tracks = await GetTracksAsync(url, cancellationToken: cancellationToken);
            playlist.Tracks = tracks.ToList();

            foreach (var track in playlist.Tracks)
                track.PlaylistName = playlist.Title;
        }

        return playlist;
    }

    /// <summary>
    /// Enumerates tracks included in the specified playlist url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public async IAsyncEnumerable<Batch<Track>> GetTrackBatchesAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!await IsUrlValidAsync(url, cancellationToken))
            throw new SoundcloudExplodeException("Invalid playlist url");

        var playlist = await GetAsync(url, false, cancellationToken);
        if (playlist is null || playlist.Tracks is null)
            yield break;

        if (offset > 0)
            playlist.Tracks = playlist.Tracks.Skip(offset).ToList();

        // Soundcloud single request limit is 50
        foreach (var chunk in playlist.Tracks.ChunkBy(50))
        {
            var ids = chunk.Select(x => x.Id).ToList();
            var idsStr = string.Join(",", ids);

            // Tracks are returned unordered here even though the ids are in the right order in the url
            var response = await _http.ExecuteGetAsync(
                $"https://api-v2.soundcloud.com/tracks?ids={idsStr}&limit={limit}&offset={offset}&client_id={Constants.ClientId}",
                cancellationToken
            );

            var tracks = JsonSerializer.Deserialize<List<Track>>(response)!;
            foreach (var track in tracks)
                track.PlaylistName = playlist.Title;

            // Set the right order
            tracks = tracks.OrderBy(x => ids.IndexOf(x.Id)).ToList();

            yield return Batch.Create(tracks);
        }
    }

    /// <summary>
    /// Enumerates tracks included in the specified playlist url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Track> GetTracksAsync(
        string url,
        int offset = Constants.DefaultOffset,
        int limit = Constants.DefaultLimit,
        CancellationToken cancellationToken = default) =>
        GetTrackBatchesAsync(url, offset, limit, cancellationToken).FlattenAsync();

    /// <summary>
    /// Enumerates tracks included in the specified playlist url.
    /// </summary>
    /// <exception cref="SoundcloudExplodeException"/>
    public IAsyncEnumerable<Track> GetTracksAsync(
        string url,
        CancellationToken cancellationToken) =>
        GetTrackBatchesAsync(url, cancellationToken: cancellationToken).FlattenAsync();
}