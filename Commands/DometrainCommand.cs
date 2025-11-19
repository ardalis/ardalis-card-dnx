using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class DometrainCommand : Command
{
    private readonly PostHogService _postHog;

    public DometrainCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("dometrain");
        var url = "https://dometrain.com/author/steve-ardalis-smith/";
        AnsiConsole.MarkupLine($"[bold purple]Opening Dometrain Author profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
