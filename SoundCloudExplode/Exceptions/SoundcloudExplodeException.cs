using System;

namespace SoundCloudExplode.Exceptions;

/// <summary>
/// Exception thrown within <see cref="SoundCloudExplode"/>.
/// </summary>
/// <remarks>
/// Initializes an instance of <see cref="SoundcloudExplodeException"/>.
/// </remarks>
/// <param name="message"></param>
public class SoundcloudExplodeException(string message) : Exception(message) { }
