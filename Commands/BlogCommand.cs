using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ardalis.Commands;

public class BlogCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var url = "https://ardalis.com";
        AnsiConsole.MarkupLine($"[bold green]Opening blog:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
