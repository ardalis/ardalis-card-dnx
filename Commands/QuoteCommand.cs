using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class QuoteCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        var quote = await QuoteHelper.GetRandomQuote();
        AnsiConsole.WriteLine($"\"{quote}\" - Ardalis");
        return 0;
    }
}
