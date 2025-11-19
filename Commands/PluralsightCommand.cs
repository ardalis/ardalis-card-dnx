using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class PluralsightCommand : Command
{
    private readonly PostHogService _postHog;

    public PluralsightCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("pluralsight");
        var url = "https://www.pluralsight.com/authors/steve-smith";
        AnsiConsole.MarkupLine($"[bold orange1]Opening Pluralsight profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
