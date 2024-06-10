namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested track is unavailable/blocked from country.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="TrackUnavailableException"/>.
/// </remarks>
public class TrackUnavailableException(string message) : SoundcloudExplodeException(message) { }
