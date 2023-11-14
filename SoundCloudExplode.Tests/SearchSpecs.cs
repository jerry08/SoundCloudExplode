using System.Threading.Tasks;
using FluentAssertions;
using SoundCloudExplode.Common;
using Xunit;

namespace SoundCloudExplode.Tests;

public class SearchSpecs
{
    [Fact]
    public async Task I_can_get_results_from_a_search_query()
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var results = await soundcloud.Search.GetResultsAsync("adele").CollectAsync(10);

        // Assert
        results.Should().HaveCountGreaterOrEqualTo(10);
    }

    [Fact]
    public async Task I_can_get_track_results_from_a_search_query()
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var videos = await soundcloud.Search.GetTracksAsync("adele").CollectAsync(10);

        // Assert
        videos.Should().HaveCountGreaterOrEqualTo(50);
    }

    [Fact]
    public async Task I_can_get_playlist_results_from_a_search_query()
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var playlists = await soundcloud.Search.GetPlaylistsAsync("adele").CollectAsync(10);

        // Assert
        playlists.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_album_results_from_a_search_query()
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var channels = await soundcloud.Search.GetAlbumsAsync("adele").CollectAsync(10);

        // Assert
        channels.Should().NotBeEmpty();
    }

    [Fact]
    public async Task I_can_get_user_results_from_a_search_query()
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var channels = await soundcloud.Search.GetUsersAsync("adele").CollectAsync(10);

        // Assert
        channels.Should().NotBeEmpty();
    }
}
