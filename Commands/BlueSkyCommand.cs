using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class BlueSkyCommand : Command
{
    private readonly PostHogService _postHog;

    public BlueSkyCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("bluesky");
        var url = "https://bsky.app/profile/ardalis.com";
        AnsiConsole.MarkupLine($"[bold blue]Opening Bluesky profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
