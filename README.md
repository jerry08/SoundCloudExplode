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

var single_track = await soundcloud.GetTrackAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");
//Or
var single_track = await soundcloud.GetTracksAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var playlist_tracks = await soundcloud.GetTracksAsync("https://soundcloud.com/tommy-enjoy/sets/aimer");
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
