# SoundCloudExplode
[![Version](https://img.shields.io/nuget/v/SoundCloudExplode.svg)](https://nuget.org/packages/SoundCloudExplode)
[![Downloads](https://img.shields.io/nuget/dt/SoundCloudExplode.svg)](https://nuget.org/packages/SoundCloudExplode)

**SoundCloudExplode** is a library that provides an interface to query metadata of SoundCloud tracks and playlistschannels, as well as to resolve and download streams.

### 🌟STAR THIS REPOSITORY TO SUPPORT THE DEVELOPER AND ENCOURAGE THE DEVELOPMENT OF THE APPLICATION!


## Install

- 📦 [NuGet](https://nuget.org/packages/SoundCloudExplode): `dotnet add package SoundCloudExplode` (**main package**)

## Usage

**SoundCloudExplode** exposes its functionality through a single entry point — the `SoundCloudClient` class.
Create an instance of this class and use the provided operations to send requests.

### Tracks

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var track = await soundcloud.Tracks.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var tracks = await soundcloud.GetTracksAsync("https://soundcloud.com/tommy-enjoy/sets/aimer");
//Or
var tracks = new List<TrackInformation>();
var playlist = await soundcloud.GetPlaylistAsync("https://soundcloud.com/tommy-enjoy/sets/aimer");
foreach (var track in playlist.Tracks)
{
    var trackUrl = await soundcloud.QueryTrackUrlAsync(track.Id);
    var trackInfo = await soundcloud.GetTrackAsync(trackUrl);

    tracks.Add(trackInfo);
}
```

### Playlists

#### Retrieving playlist metadata

You can get the metadata associated with a Soundcloud playlist by calling `Playlists.GetAsync(...)` method:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var playlist = await soundcloud.Playlists.GetAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
);

var title = playlist.Title;
var artworkUrl = playlist.ArtworkUrl;
...
```

#### Getting tracks included in a playlist

To get the tracks included in a playlist, call `Playlists.GetTracksAsync(...)`:

```csharp
using SoundCloudExplode;
using SoundCloudExplode.Common;

var soundcloud = new SoundCloudClient();

// Get all playlist tracks
var tracks = await soundcloud.Playlists.GetTracksAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
);

// Get only the first 20 playlist tracks
var tracksSubset = await soundcloud.Playlists
    .GetTracksAsync("https://soundcloud.com/tommy-enjoy/sets/aimer")
    .CollectAsync(20);
```

#### Downloading tracks

```csharp
using System;
using System.IO;
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var tracks = await soundcloud.GetTracksAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

foreach (var track in tracks)
{
    var trackName = string.Join("_", track.Title.Split(Path.GetInvalidFileNameChars()));

    await soundcloud.DownloadAsync(track, $@"{Environment.CurrentDirectory}\Download\{trackName}.mp3");
}
```
