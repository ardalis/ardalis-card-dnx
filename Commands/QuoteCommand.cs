using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class QuoteCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        var quote = await QuoteHelper.GetRandomQuote();
        AnsiConsole.WriteLine($"\"{quote}\" - Ardalis");
        return 0;
    }
}
