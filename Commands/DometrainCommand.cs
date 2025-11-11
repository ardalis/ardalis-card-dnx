using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class DometrainCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://dometrain.com/author/steve-ardalis-smith/";
        AnsiConsole.MarkupLine($"[bold purple]Opening Dometrain profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
