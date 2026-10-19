using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SoundCloudExplode.Common;
using SoundCloudExplode.Demo.Cli.Utils;
using SoundCloudExplode.Tracks;
using Spectre.Console;

namespace SoundCloudExplode.Demo.Cli;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        AnsiConsole.Write(new Rule("[orange1]SoundCloudExplode[/]").LeftJustified());
        if (args is ["--help"] or ["-h"])
        {
            ShowHelp();
            return 0;
        }
        if (
            (args.Length == 0 && Console.IsInputRedirected)
            || (args.Length > 0 && args[0].StartsWith('-') && args[0] != "--samples")
        )
        {
            ShowHelp();
            return 1;
        }
        using var cancellation = new CancellationTokenSource();
        ConsoleCancelEventHandler onCancel = (_, e) =>
        {
            e.Cancel = true;
            cancellation.Cancel();
        };
        Console.CancelKeyPress += onCancel;
        try
        {
            var soundcloud = new SoundCloudClient();
            await AnsiConsole
                .Status()
                .StartAsync(
                    "Connecting to SoundCloud...",
                    _ => soundcloud.InitializeAsync(cancellation.Token)
                );
            if (args is ["--samples"])
            {
                await Samples.RunAsync(soundcloud);
                return 0;
            }
            if (args.Length > 0)
            {
                foreach (var url in args)
                    await DownloadUrlAsync(soundcloud, url, cancellation.Token);
                return 0;
            }
            AnsiConsole.MarkupLine(
                "[grey]Enter a track or playlist URL. Type q or press Ctrl+C to exit.[/]"
            );
            while (!cancellation.IsCancellationRequested)
            {
                var url = await AnsiConsole.PromptAsync(
                    new TextPrompt<string>("[cyan]SoundCloud URL:[/]").Validate(value =>
                        value.Equals("q", StringComparison.OrdinalIgnoreCase)
                        || IsSoundCloudUrl(value)
                            ? ValidationResult.Success()
                            : ValidationResult.Error("Enter an absolute SoundCloud URL or q.")
                    ),
                    cancellation.Token
                );
                if (url.Equals("q", StringComparison.OrdinalIgnoreCase))
                    break;
                try
                {
                    await DownloadUrlAsync(soundcloud, url, cancellation.Token);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    AnsiConsole.MarkupLineInterpolated($"[red]Download failed:[/] {ex.Message}");
                }
            }
            return 0;
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            AnsiConsole.MarkupLine("[yellow]Cancelled.[/]");
            return 130;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLineInterpolated($"[red]Error:[/] {ex.Message}");
            return 1;
        }
        finally
        {
            Console.CancelKeyPress -= onCancel;
        }
    }

    private static async Task DownloadUrlAsync(
        SoundCloudClient soundcloud,
        string url,
        CancellationToken cancellationToken
    )
    {
        if (!IsSoundCloudUrl(url))
            throw new ArgumentException("Enter an absolute SoundCloud track or playlist URL.");
        var kind = await AnsiConsole
            .Status()
            .StartAsync(
                "Resolving URL...",
                _ => soundcloud.GetUrlKindAsync(url, cancellationToken)
            );
        if (kind == Kind.Track)
        {
            var track =
                await soundcloud.Tracks.GetAsync(url, cancellationToken)
                ?? throw new InvalidOperationException("The track was not found.");
            await DownloadTrackAsync(soundcloud, track, cancellationToken);
        }
        else if (kind is Kind.Playlist or Kind.Album)
        {
            var tracks = await AnsiConsole
                .Status()
                .StartAsync(
                    "Loading playlist...",
                    async _ => await soundcloud.Playlists.GetTracksAsync(url, cancellationToken)
                );
            AnsiConsole.MarkupLineInterpolated($"[green]Found {tracks.Count} tracks.[/]");
            foreach (var track in tracks)
                await DownloadTrackAsync(soundcloud, track, cancellationToken);
        }
        else
        {
            throw new ArgumentException(
                "This URL identifies a user. Enter a track or playlist URL."
            );
        }
    }

    private static async Task DownloadTrackAsync(
        SoundCloudClient soundcloud,
        Track track,
        CancellationToken cancellationToken
    )
    {
        var transcoding = track.GetPreferredTranscoding();
        var name = PathEx.EscapeFileName(track.Title ?? track.Id.ToString()).TrimEnd('.', ' ');
        if (string.IsNullOrWhiteSpace(name))
            name = track.Id.ToString();
        var path = Path.Combine(
            Environment.CurrentDirectory,
            "Downloads",
            $"{name}-{track.Id}{transcoding?.GetFileExtension() ?? ".audio"}"
        );
        var table = new Table().AddColumn("Track").AddColumn("Artist").AddColumn("Format");
        table.AddRow(
            (track.Title ?? "Untitled").EscapeMarkup(),
            (track.User?.Username ?? "Unknown").EscapeMarkup(),
            (transcoding?.Format?.MimeType ?? "Unavailable").EscapeMarkup()
        );
        AnsiConsole.Write(table);
        await AnsiConsole
            .Progress()
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn()
            )
            .StartAsync(async context =>
            {
                var task = context.AddTask($"[cyan]{name.EscapeMarkup()}[/]", maxValue: 1);
                await soundcloud.DownloadAsync(
                    track,
                    path,
                    task,
                    cancellationToken: cancellationToken
                );
                task.Value = 1;
                task.StopTask();
            });
        AnsiConsole.MarkupLineInterpolated($"[green]Saved:[/] {path}");
    }

    private static bool IsSoundCloudUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https"
        && (
            uri.Host.Equals("soundcloud.com", StringComparison.OrdinalIgnoreCase)
            || uri.Host.EndsWith(".soundcloud.com", StringComparison.OrdinalIgnoreCase)
        );

    private static void ShowHelp()
    {
        AnsiConsole.MarkupLine(
            "[bold]Usage:[/] dotnet run --project SoundCloudExplode.Demo.Cli -- [[URL ...]]"
        );
        AnsiConsole.MarkupLine(
            "Run without arguments for interactive prompts. Downloads are saved in ./Downloads."
        );
        AnsiConsole.MarkupLine("[grey]--help[/]     Show this help.");
        AnsiConsole.MarkupLine("[grey]--samples[/]  Run the existing API examples.");
    }
}
