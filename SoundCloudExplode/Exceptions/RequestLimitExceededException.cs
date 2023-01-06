namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown when Soundcloud denies a request because the client has exceeded rate limit.
/// </summary>
public class RequestLimitExceededException : SoundcloudExplodeException
{
    /// <summary>
    /// Initializes an instance of <see cref="RequestLimitExceededException" />.
    /// </summary>
    public RequestLimitExceededException(string message) : base(message)
    {
    }
}