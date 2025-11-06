using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class BlogCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://ardalis.com";
        AnsiConsole.MarkupLine($"[bold green]Opening blog:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
