<h1 align="center">
    SoundCloudExplode
</h1>

<p align="center">
   <a href="https://discord.gg/mhxsSMy2Nf"><img src="https://img.shields.io/badge/Discord-7289DA?style=for-the-badge&logo=discord&logoColor=white"></a>
   <a href="https://github.com/jerry08/SoundCloudExplode"><img src="https://img.shields.io/nuget/dt/SoundCloudExplode.svg?label=Downloads&color=%233DDC84&logo=nuget&logoColor=%23fff&style=for-the-badge"></a>
</p>

**SoundCloudExplode** is a library that provides an interface to query metadata of SoundCloud tracks and playlists, as well as to resolve and download streams.

### 🌟STAR THIS REPOSITORY TO SUPPORT THE DEVELOPER AND ENCOURAGE THE DEVELOPMENT OF THE APPLICATION!


## Install

- 📦 [NuGet](https://nuget.org/packages/SoundCloudExplode): `dotnet add package SoundCloudExplode` (**main package**)

## Usage

**SoundCloudExplode** exposes its functionality through a single entry point — the `SoundCloudClient` class.
Create an instance of this class and use the provided operations to send requests.

### Tracks

#### Retrieving track metadata

To retrieve the metadata associated with a Soundcloud track, call `Tracks.GetAsync(...)`:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

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

// Speed up process by increasing 'maxConcurrent' (default is 1)
var tracksSubset = await soundcloud.Playlists
    .GetTracksAsync(
        "https://soundcloud.com/tommy-enjoy/sets/aimer",
        maxConcurrent: 10
    )
    .CollectAsync(20);

// Loop through large playlist faster by setting 'maxChunks' (default is 1)
var tracksSubset = await soundcloud.Playlists
    .GetTracksAsync(
        "https://soundcloud.com/tommy-enjoy/sets/aimer",
        maxConcurrent: 20,
        maxChunks: 250
    )
    .CollectAsync(100);
```

You can also enumerate the tracks iteratively without waiting for the whole list to load:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

await foreach (var track in soundcloud.Playlists.GetTracksAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
))
{
    var title = track.Title;
    var duration = track.Duration;
}
```

If you need precise control over how many requests you send to Soundcloud, use `Playlists.GetTrackBatchesAsync(...)` which returns tracks wrapped in batches:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

// Each batch corresponds to one request
await foreach (var batch in soundcloud.Playlists.GetTrackBatchesAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
))
{
    foreach (var track in batch.Items)
    {
        var title = track.Title;
        var duration = track.Duration;
    }
}
```

#### Downloading tracks

```csharp
using System;
using System.IO;
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var track = await soundcloud.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var trackName = string.Join("_", track.Title.Split(Path.GetInvalidFileNameChars()));

await soundcloud.DownloadAsync(track, $@"{Environment.CurrentDirectory}\Download\{trackName}.mp3");
```


You can request the download url for a particular track by calling `Tracks.GetDownloadUrlAsync(...)`:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var track = await soundcloud.Tracks.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

var downloadUrl = await soundcloud.Tracks.GetDownloadUrlAsync(
    track
);
```
