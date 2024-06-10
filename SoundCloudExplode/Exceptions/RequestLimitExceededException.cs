namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown when Soundcloud denies a request because the client has exceeded rate limit.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="RequestLimitExceededException" />.
/// </remarks>
public class RequestLimitExceededException(string message) : SoundcloudExplodeException(message) { }
