using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class TipsCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        var tip = await TipHelper.GetRandomTip();
        
        var panel = new Panel(new Markup(
            $"[bold yellow]💡 Coding Tip[/]\n\n" +
            $"{tip.TipText}\n\n" +
            $"[dim]Learn more:[/] [link={tip.ReferenceLink}]{tip.ReferenceLink}[/]"
        ))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Yellow),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(panel);
        
        return 0;
    }
}
