using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class ContactCommand : Command
{
    private readonly PostHogService _postHog;

    public ContactCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("contact");
        var url = "https://ardalis.com/contact/";
        AnsiConsole.MarkupLine($"[bold blue]Opening contact page:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
