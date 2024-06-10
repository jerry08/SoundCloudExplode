namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested playlist is unavailable.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="PlaylistUnavailableException"/>.
/// </remarks>
public class PlaylistUnavailableException(string message) : SoundcloudExplodeException(message) { }
