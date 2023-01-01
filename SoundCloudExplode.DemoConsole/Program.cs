using System;
using System.IO;
using System.Threading.Tasks;
using SoundCloudExplode.DemoConsole.Utils;

namespace SoundCloudExplode.DemoConsole;

internal static class Program
{
    public static async Task Main()
    {
        Console.Title = "SoundCloudExplode Demo";

        var soundcloud = new SoundCloudClient();

        // Get the track URL
        Console.Write("Enter Soundcloud track URL: ");
        var url = Console.ReadLine() ?? "";

        var track = await soundcloud.Tracks.GetAsync(url);
        if (track is null)
        {
            Console.Error.WriteLine("The track is not found.");
            return;
        }

        // Download the stream
        var fileName = $@"{Environment.CurrentDirectory}\Download\{ReplaceInvalidChars(track.Title!)}.mp3";

        using (var progress = new ConsoleProgress())
            await soundcloud.DownloadAsync(track, fileName, progress);

        Console.WriteLine("Done");
        Console.WriteLine($"Track saved to '{fileName}'");
    }

    public static string ReplaceInvalidChars(string fileName)
    {
        return string.Join("_", fileName.Split(Path.GetInvalidFileNameChars()));
    }
}