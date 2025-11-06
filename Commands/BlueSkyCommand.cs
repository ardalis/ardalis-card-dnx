using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class BlueSkyCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://bsky.app/profile/ardalis.com";
        AnsiConsole.MarkupLine($"[bold blue]Opening Bluesky profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
