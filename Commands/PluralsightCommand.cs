using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class PluralsightCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://www.pluralsight.com/authors/steve-smith";
        AnsiConsole.MarkupLine($"[bold orange1]Opening Pluralsight profile:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
