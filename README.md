<h1 align="center">
    SoundCloudExplode
</h1>

<p align="center">
   <a href="https://discord.gg/U7XweVubJN"><img src="https://img.shields.io/badge/Discord-7289DA?style=for-the-badge&logo=discord&logoColor=white"></a>
   <a href="https://github.com/jerry08/SoundCloudExplode"><img src="https://img.shields.io/nuget/dt/SoundCloudExplode.svg?label=Downloads&color=%233DDC84&logo=nuget&logoColor=%23fff&style=for-the-badge"></a>
</p>

**SoundCloudExplode** is a library that provides an interface to query metadata of SoundCloud tracks and playlists, as well as to resolve and download streams.

### 🌟STAR THIS REPOSITORY TO SUPPORT THE DEVELOPER AND ENCOURAGE THE DEVELOPMENT OF THE APPLICATION!


## Install

- 📦 [NuGet](https://nuget.org/packages/SoundCloudExplode): `dotnet add package SoundCloudExplode` (**main package**)

## Usage

**SoundCloudExplode** exposes its functionality through a single entry point — the `SoundCloudClient` class.
Create an instance of this class and use the provided operations to send requests.
Call `await soundcloud.InitializeAsync()` first to obtain SoundCloud's current public client ID.

### Tracks

#### Retrieving track metadata

To retrieve the metadata associated with a Soundcloud track, call `Tracks.GetAsync(...)`:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();

var track = await soundcloud.Tracks.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var title = track.Title;
var duration = track.Duration;
```

### Playlists

#### Retrieving playlist metadata

You can get the metadata associated with a Soundcloud playlist by calling `Playlists.GetAsync(...)` method:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();

// Get a playlist basic information
var playlist = await soundcloud.Playlists.GetAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
);

// Or get playlist and load all related tracks at the same time
var playlist = await soundcloud.Playlists.GetAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer",
    true
);

var title = playlist.Title;
var artworkUrl = playlist.ArtworkUrl;
var tracks = playlist.Tracks;
...
```

#### Getting tracks included in a playlist

To get the tracks included in a playlist, call `Playlists.GetTracksAsync(...)`:

```csharp
using SoundCloudExplode;
using SoundCloudExplode.Common;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();
var playlistUrl = "https://soundcloud.com/tommy-enjoy/sets/aimer";

// Get all playlist tracks
var tracks = await soundcloud.Playlists.GetTracksAsync(playlistUrl);

// Get only the first 20 playlist tracks
var tracksSubset = await soundcloud.Playlists.GetTracksAsync(playlistUrl).CollectAsync(20);

// Get only the first 20 playlist tracks (by setting a limit)
var tracksSubset = await soundcloud.Playlists.GetTracksAsync(playlistUrl, limit: 20);

// Setting offset
var tracksSubset = await soundcloud.Playlists.GetTracksAsync(
    playlistUrl,
    offset: 10,
    limit: 5
);
```

You can also enumerate the tracks iteratively without waiting for the whole list to load:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();
var playlistUrl = "https://soundcloud.com/tommy-enjoy/sets/aimer";

await foreach (var track in soundcloud.Playlists.GetTracksAsync(playlistUrl))
{
    var title = track.Title;
    var duration = track.Duration;
}
```

If you need precise control over how many requests you send to SoundCloud, use `Playlists.GetTrackBatchesAsync(...)` which returns tracks wrapped in batches:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();
var playlistUrl = "https://soundcloud.com/tommy-enjoy/sets/aimer";

// Each batch corresponds to one request
await foreach (var batch in soundcloud.Playlists.GetTrackBatchesAsync(playlistUrl))
{
    foreach (var track in batch.Items)
    {
        var title = track.Title;
        var duration = track.Duration;
    }
}
```

### Albums
**Note:** Use the same method as retrieving playlists to get albums because they are the same. 

### Searching
You can execute a search query and get its results by calling `Search.GetResultsAsync(...)`. Each result may represent either a track, a playlist, an album, or a user, so you need to apply pattern matching to handle the corresponding cases:

```csharp
using SoundCloudExplode;
using SoundCloudExplode.Common;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();

await foreach (var result in soundcloud.Search.GetResultsAsync("banda neira"))
{
    // Use pattern matching to handle different results (tracks, playlists, users)
    switch (result)
    {
        case TrackSearchResult track:
            {
                var id = track.Id;
                var title = track.Title;
                var duration = track.Duration;
                break;
            }
        // NOTE: Soundcloud handles playlist and albums the same way.
        case PlaylistSearchResult playlist:
            {
                var id = playlist.Id;
                var title = playlist.Title;
                break;
            }
        case UserSearchResult user:
            {
                var id = user.Id;
                var title = user.Title;
                var userName = user.Username;
                var fullName = user.FullName;
                break;
            }
    }
}
```

To limit the results to a specific type, use `Search.GetTracksAsync(...)`, `Search.GetPlaylistsAsync(...)`, or `Search.GetUsersAsync(...)`

```csharp
using SoundCloudExplode;
using SoundCloudExplode.Common;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();

var tracks = await soundcloud.Search.GetTracksAsync("banda neira");
var playlists = await soundcloud.Search.GetPlaylistsAsync("banda neira");
var users = await soundcloud.Search.GetUsersAsync("banda neira");
```

#### Downloading tracks

```csharp
using System;
using System.IO;
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();

var track = await soundcloud.Tracks.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var trackName = string.Join("_", track.Title.Split(Path.GetInvalidFileNameChars()));
var extension = track.GetPreferredTranscoding()?.GetFileExtension() ?? ".audio";

await soundcloud.DownloadAsync(track, Path.Combine("Downloads", trackName + extension));
```

You can request the download url for a particular track by calling `Tracks.GetDownloadUrlAsync(...)`:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();
await soundcloud.InitializeAsync();

var track = await soundcloud.Tracks.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var downloadUrl = await soundcloud.Tracks.GetDownloadUrlAsync(track);
```

The returned URL can point to progressive audio or an **HLS playlist**. Pass HLS URLs to a player that supports HLS, or use `DownloadAsync` to assemble the audio segments into a file. The library preserves the source encoding: AAC in an MP4 container uses `.m4a`, MP3 uses `.mp3`, and Opus in Ogg uses `.ogg`. It does not transcode audio.

`DownloadAsync` supports complete, unencrypted HLS playlists, including fragmented MP4 initialization segments and adaptive playlists. It reports unsupported encryption, byte ranges, discontinuities, or incomplete/live playlists explicitly. A failed or cancelled download may leave a partial file.

### Demo CLI

The demo uses Spectre.Console prompts, metadata tables, status indicators, and download progress. Run it with the .NET 10 SDK:

```sh
dotnet run --project SoundCloudExplode.Demo.Cli
dotnet run --project SoundCloudExplode.Demo.Cli -- "https://soundcloud.com/artist/track"
```

It accepts tracks and playlists, saves files in `Downloads` using the source format's extension, and exits with `q` or Ctrl+C. Use `--help` for usage or `--samples` to run the API examples.

### Development and tests

Package versions are maintained in `Directory.Packages.props`. Building the solution requires the .NET 10 SDK; the library targets .NET Standard 2.0, .NET 9, and .NET 10.

The default test run uses local HTTP fixtures. Live SoundCloud tests are skipped unless explicitly enabled:

```sh
dotnet test
```

To run live integration tests in PowerShell:

```powershell
$env:SOUNDCLOUD_INTEGRATION_TESTS = "1"
dotnet test --filter "Category=Integration"
Remove-Item Env:SOUNDCLOUD_INTEGRATION_TESTS
```

Live tests depend on SoundCloud availability, geographic restrictions, and the continued availability of the sample tracks. CI runs the offline suite with coverage before creating release packages.
