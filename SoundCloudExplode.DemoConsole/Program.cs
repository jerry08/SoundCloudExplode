using System;
using System.IO;
using System.Threading.Tasks;

namespace SoundCloudExplode.DemoConsole
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.Title = "SoundCloudExplode Demo";

            var soundCloud = new SoundCloudClient();

            // Read the track/playlist url
            Console.Write("Enter SoundCloud track/playlist URL: ");
            var url = Console.ReadLine() ?? "";

            var tracks = await soundCloud.GetTracksAsync(url);

            foreach (var track in tracks)
            {
                await soundCloud.DownloadAsync(track, $@"{Environment.CurrentDirectory}\Download\{ReplaceInvalidChars(track.Title!)}.mp3");
            }

            return 0;
        }

        public static string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}