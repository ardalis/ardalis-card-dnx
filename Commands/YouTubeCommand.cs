using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ardalis.Commands;

public class YouTubeCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var url = "https://youtube.com/@Ardalis";
        AnsiConsole.MarkupLine($"[bold red]Opening YouTube channel:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
