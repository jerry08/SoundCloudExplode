using System.Threading.Tasks;
using FluentAssertions;
using SoundCloudExplode.Common;
using Xunit;

namespace SoundCloudExplode.Tests;

public class SearchSpecs
{
    [Theory]
    [InlineData("adele")]
    public async Task I_can_get_results_from_a_search_query(string query)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var results = await soundcloud.Search.GetResultsAsync(query).CollectAsync(10);

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Theory]
    [InlineData("adele")]
    public async Task I_can_get_track_results_from_a_search_query(string query)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var videos = await soundcloud.Search.GetTracksAsync(query).CollectAsync(10);

        // Assert
        videos.Should().HaveCountGreaterThanOrEqualTo(10);
    }

    [Theory]
    [InlineData("adele")]
    public async Task I_can_get_playlist_results_from_a_search_query(string query)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var playlists = await soundcloud.Search.GetPlaylistsAsync(query).CollectAsync(10);

        // Assert
        playlists.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("adele")]
    public async Task I_can_get_album_results_from_a_search_query(string query)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var channels = await soundcloud.Search.GetAlbumsAsync(query).CollectAsync(10);

        // Assert
        channels.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("adele")]
    public async Task I_can_get_user_results_from_a_search_query(string query)
    {
        // Arrange
        var soundcloud = new SoundCloudClient();

        // Act
        var channels = await soundcloud.Search.GetUsersAsync(query).CollectAsync(10);

        // Assert
        channels.Should().NotBeEmpty();
    }
}
