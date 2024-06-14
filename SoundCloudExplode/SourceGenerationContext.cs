using System.Collections.Generic;
using System.Text.Json.Serialization;
using SoundCloudExplode.Playlists;
using SoundCloudExplode.Search;
using SoundCloudExplode.Tracks;
using SoundCloudExplode.Users;

namespace SoundCloudExplode;

[JsonSerializable(typeof(Track))]
[JsonSerializable(typeof(Playlist))]
[JsonSerializable(typeof(User))]
[JsonSerializable(typeof(List<Track>))]
[JsonSerializable(typeof(List<Playlist>))]
[JsonSerializable(typeof(TrackSearchResult))]
[JsonSerializable(typeof(PlaylistSearchResult))]
[JsonSerializable(typeof(UserSearchResult))]
internal partial class SourceGenerationContext : JsonSerializerContext;
