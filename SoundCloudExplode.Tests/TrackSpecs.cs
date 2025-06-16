using System.Threading.Tasks;
using FluentAssertions;
using SoundCloudExplode.Common;
using Xunit;

namespace SoundCloudExplode.Tests;

public class TrackSpecs
{
    [Theory]
    [InlineData("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description")]
    public async Task I_can_get_the_metadata_of_any_available_track(string url)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();
        await soundcloud.InitializeAsync();

        // Act
        var results = await soundcloud.Tracks.GetAsync(url);

        // Assert
        results.Should().NotBeNull();
    }

    [Theory]
    [InlineData("https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description")]
    public async Task I_can_get_the_download_url_of_a_track(string url)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();
        await soundcloud.InitializeAsync();

        // Act
        var result = await soundcloud.Tracks.GetDownloadUrlAsync(url);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}
