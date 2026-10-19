using System;
using Xunit;

namespace SoundCloudExplode.Tests;

public sealed class IntegrationTheoryAttribute : TheoryAttribute
{
    public IntegrationTheoryAttribute()
    {
        if (Environment.GetEnvironmentVariable("SOUNDCLOUD_INTEGRATION_TESTS") != "1")
            Skip = "Set SOUNDCLOUD_INTEGRATION_TESTS=1 to run live SoundCloud tests.";
    }
}
