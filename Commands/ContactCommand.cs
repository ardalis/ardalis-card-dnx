using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class ContactCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        var url = "https://ardalis.com/contact/";
        AnsiConsole.MarkupLine($"[bold blue]Opening contact page:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}
