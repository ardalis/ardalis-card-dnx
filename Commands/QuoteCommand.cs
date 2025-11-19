using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class QuoteCommand : AsyncCommand
{
    private readonly PostHogService _postHog;

    public QuoteCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("quote");
        var quote = await QuoteHelper.GetRandomQuote();
        AnsiConsole.WriteLine($"\"{quote}\" - Ardalis");
        return 0;
    }
}
