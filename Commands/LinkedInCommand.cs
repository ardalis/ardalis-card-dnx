using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class LinkedInCommand : Command
{
    private readonly PostHogService _postHog;

    public LinkedInCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("linkedin");
        var url = "https://www.linkedin.com/in/stevenandrewsmith/";
        AnsiConsole.MarkupLine($"[bold blue]Opening LinkedIn profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
