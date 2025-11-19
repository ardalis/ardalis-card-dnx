using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class SubscribeCommand : Command
{
    private readonly PostHogService _postHog;

    public SubscribeCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("subscribe");
        var url = "https://ardalis.com/tips";
        AnsiConsole.MarkupLine($"[bold green]Opening newsletter subscription page:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
