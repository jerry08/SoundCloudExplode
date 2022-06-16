using System;

namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown within <see cref="SoundCloudExplode"/>.
/// </summary>
public class SoundCloudExplodeException : Exception
{
    /// <summary>
    /// Initializes an instance of <see cref="SoundCloudExplodeException"/>.
    /// </summary>
    /// <param name="message"></param>
    public SoundCloudExplodeException(string message) : base(message)
    {
    }
}