using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class NimbleProCommand : Command
{
    private readonly PostHogService _postHog;

    public NimbleProCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("nimblepros");
        var url = "https://nimblepros.com";
        AnsiConsole.MarkupLine($"[bold green]Opening NimblePros:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
