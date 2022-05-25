# SoundCloudExplode
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

var track = await soundcloud.GetTracksAsync("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description");
var playlist_tracks = await soundcloud.GetTracksAsync("https://soundcloud.com/tommy-enjoy/sets/aimer");
```