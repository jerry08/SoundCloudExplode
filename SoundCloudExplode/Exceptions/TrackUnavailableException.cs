namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown when the requested track is unavailable/blocked from country.
/// </summary>
public class TrackUnavailableException : SoundcloudExplodeException
{
    /// <summary>
    /// Initializes an instance of <see cref="TrackUnavailableException"/>.
    /// </summary>
    public TrackUnavailableException(string message) : base(message)
    {
    }
}