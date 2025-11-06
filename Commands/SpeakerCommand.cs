using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class SpeakerCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://sessionize.com/ardalis";
        AnsiConsole.MarkupLine($"[bold yellow]Opening speaker profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
