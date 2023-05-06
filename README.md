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

//Get playlist info with all tracks
var playlist = await soundcloud.Playlists.GetAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
);

//Or Get only the playlist basic info without loading all tracks info
var playlist = await soundcloud.Playlists.GetAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer",
    false
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

// Get all tracks in a playlist
var tracks = await soundcloud.Playlists.GetTracksAsync(
    "https://soundcloud.com/tommy-enjoy/sets/aimer"
);

// Get only the first 20 playlist tracks
var tracksSubset = await soundcloud.Playlists
    .GetTracksAsync(
        "https://soundcloud.com/tommy-enjoy/sets/aimer",
        limit: 20
    );

//Setting offset
var tracksSubset = await soundcloud.Playlists
    .GetTracksAsync(
        "https://soundcloud.com/tommy-enjoy/sets/aimer",
        offset: 10,
        limit: 5
    );
```

### Albums
**Note:** Use the same method as retrieving playlists to get albums because they are the same. 

### Searching
You can execute a search query and get its results by calling Search.GetResultsAsync(...). Each result may represent either a track, a playlist, an album, or a user, so you need to apply pattern matching to handle the corresponding cases:

```csharp
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var results = await soundcloud.Search.GetResultsAsync("banda neira");

foreach (var result in results)
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
        //NOTE: Soundcloud handles playlist and albums the same way.
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

#### Downloading tracks

```csharp
using System;
using System.IO;
using SoundCloudExplode;

var soundcloud = new SoundCloudClient();

var track = await soundcloud.Tracks.GetAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");

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

**This project was inspired by [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode).**
