using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class YouTubeCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://youtube.com/@Ardalis";
        AnsiConsole.MarkupLine($"[bold red]Opening YouTube channel:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
