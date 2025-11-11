using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class NimbleProCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://nimblepros.com";
        AnsiConsole.MarkupLine($"[bold green]Opening NimblePros:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
