using System;

namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown within <see cref="SoundCloudExplode"/>.
/// </summary>
public class SoundcloudExplodeException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="SoundcloudExplodeException"/>.
    /// </summary>
    /// <param name="message"></param>
    public SoundcloudExplodeException(string message) : base(message)
    {
    }
}