using System.Threading.Tasks;
using SoundCloudExplode.Common;
using Spectre.Console;

namespace SoundCloudExplode.Demo.Cli;

// Keep the existing local API experiments available via --samples.
internal static class Samples
{
    public static async Task RunAsync(SoundCloudClient soundcloud)
    {
        var tt = await soundcloud.Tracks.GetDownloadUrlAsync(
            "https://soundcloud.com/roger-lenang-pokger/6a996f60-cb27-47d6-9007"
        );

        var url1 = "https://soundcloud.com/user-507763644";
        var resolvedJson = await soundcloud.GetUrlKindAsync(url1);

        var user1 = await soundcloud.Users.GetAsync("https://soundcloud.com/user-507763644");

        var likedTracks = await soundcloud.Users.GetLikedTracksAsync(
            "https://soundcloud.com/user-507763644"
        );

        var tracksValid = await soundcloud.Tracks.IsUrlValidAsync(
            "https://soundcloud.com/user-507763644/likes"
        );

        var track2 = await soundcloud.Tracks.GetAsync(
            "https://soundcloud.com/purityy79/dororo-op-piano-sheet-in-description"
        );

        AnsiConsole.MarkupLineInterpolated($"Stream: {tt ?? "unavailable"}");
        AnsiConsole.MarkupLineInterpolated($"Profile: {user1?.Username} ({resolvedJson})");
        AnsiConsole.MarkupLineInterpolated($"Liked tracks: {likedTracks.Count}");
        AnsiConsole.MarkupLineInterpolated($"Likes URL passes track validation: {tracksValid}");
        AnsiConsole.MarkupLineInterpolated($"Track: {track2?.Title}");
    }
}
