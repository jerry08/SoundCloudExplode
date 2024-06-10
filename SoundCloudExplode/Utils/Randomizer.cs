using System;
using System.Security.Cryptography;

namespace SoundCloudExplode.Utils;

/// <summary>
/// Wrapper class for thread-safe generation of pseudo-random numbers.
/// Lazy-load singleton for ThreadStatic <see cref="Random"/>.
/// </summary>
public static class Randomizer
{
    private static Random Generate()
    {
        var buffer = new byte[4];

        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(buffer);

        return new Random(BitConverter.ToInt32(buffer, 0));
    }

    public static Random Instance => _rand ??= Generate();

    [ThreadStatic]
    private static Random _rand = default!;
}
