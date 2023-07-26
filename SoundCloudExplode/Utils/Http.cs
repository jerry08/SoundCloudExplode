using System;
using System.Net.Http;

namespace SoundCloudExplode.Utils;

internal static class Http
{
    private static readonly Lazy<HttpClient> HttpClientLazy = new(() =>
    {
        var handler = new HttpClientHandler();

        return new HttpClient(handler, true);
    });

    public static HttpClient Client => HttpClientLazy.Value;

    public static string ChromeUserAgent()
    {
        int major = Randomizer.Instance.Next(62, 70);
        int build = Randomizer.Instance.Next(2100, 3538);
        int branchBuild = Randomizer.Instance.Next(170);

        return $"Mozilla/5.0 ({RandomWindowsVersion()}) AppleWebKit/537.36 (KHTML, like Gecko) " +
            $"Chrome/{major}.0.{build}.{branchBuild} Safari/537.36";
    }

    private static string RandomWindowsVersion()
    {
        string windowsVersion = "Windows NT ";
        int random = Randomizer.Instance.Next(99) + 1;

        // Windows 10 = 45% popularity
        if (random >= 1 && random <= 45)
            windowsVersion += "10.0";

        // Windows 7 = 35% popularity
        else if (random > 45 && random <= 80)
            windowsVersion += "6.1";

        // Windows 8.1 = 15% popularity
        else if (random > 80 && random <= 95)
            windowsVersion += "6.3";

        // Windows 8 = 5% popularity
        else
            windowsVersion += "6.2";

        // Append WOW64 for X64 system
        if (Randomizer.Instance.NextDouble() <= 0.65)
            windowsVersion += Randomizer.Instance.NextDouble() <= 0.5 ? "; WOW64" : "; Win64; x64";

        return windowsVersion;
    }
}