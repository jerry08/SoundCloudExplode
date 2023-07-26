using System;
using System.IO;
using System.Threading.Tasks;
using SoundCloudExplode.Demo.Cli.Utils;
using Spectre.Console;

namespace SoundCloudExplode.Demo.Cli;

internal static class Program
{
    public static async Task Main()
    {
        Console.Title = "SoundCloudExplode Demo";

        System.Net.ServicePointManager.DefaultConnectionLimit = 200;
        //System.Net.ServicePointManager.MaxServicePoints = 500;

        var soundcloud = new SoundCloudClient();

        while (true)
        {
            Console.WriteLine("Press Ctrl+C To Exit");
            Console.WriteLine();

            // Get the track URL
            Console.Write("Enter Soundcloud track URL: ");
            var url = Console.ReadLine() ?? "";

            if (await soundcloud.Playlists.IsUrlValidAsync(url))
            {
                // Gets playlist (or album)
                //var playlist = await soundcloud.Playlists.GetAsync(url);
                var tracks = await soundcloud.Playlists.GetTracksAsync(url);

                foreach (var track in tracks)
                {
                    // Download the stream
                    var trackName = PathEx.EscapeFileName(track.Title!);
                    var trackPath = Path.Join(Environment.CurrentDirectory, "Downloads", $"{trackName}.mp3");

                    await AnsiConsole.Progress().StartAsync(async ctx =>
                    {
                        var progressTask = ctx.AddTask($"[cyan]Downloading ({trackName.EscapeMarkup()})[/]");
                        progressTask.MaxValue = 1;

                        await soundcloud.DownloadAsync(track, trackPath, progressTask);
                    });

                    Console.WriteLine("Done");
                    Console.WriteLine($"Track saved to '{trackPath}'");
                }
            }
            else if (await soundcloud.Tracks.IsUrlValidAsync(url))
            {
                var track = await soundcloud.Tracks.GetAsync(url);
                if (track is null)
                {
                    Console.Error.WriteLine("The track is not found.");
                    return;
                }

                // Download the stream
                var trackName = PathEx.EscapeFileName(track.Title!);
                var trackPath = Path.Join(Environment.CurrentDirectory, "Downloads", $"{trackName}.mp3");

                await AnsiConsole.Progress().StartAsync(async ctx =>
                {
                    var progressTask = ctx.AddTask($"[cyan]Downloading ({trackName.EscapeMarkup()})[/]");
                    progressTask.MaxValue = 1;

                    await soundcloud.DownloadAsync(track, trackPath, progressTask);
                });

                Console.WriteLine("Done");
                Console.WriteLine($"Track saved to '{trackPath}'");
            }
            else
            {
                Console.WriteLine("Bad Url");
            }
        }
    }
}