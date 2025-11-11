using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class SubscribeCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://ardalis.com/tips";
        AnsiConsole.MarkupLine($"[bold green]Opening newsletter subscription page:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
