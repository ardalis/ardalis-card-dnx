using System.Threading;
using System.Threading.Tasks;
using Ardalis.Helpers;
using TimeWarp.Nuru;
using TimeWarp.Terminal;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays a random Ardalis quote using Nuru terminal.
/// </summary>
[NuruRoute("quote", Description = "Display a random Ardalis quote")]
public sealed class QuoteEndpoint : IQuery<Unit>
{
    public sealed class Handler : IQueryHandler<QuoteEndpoint, Unit>
    {
        public async ValueTask<Unit> Handle(QuoteEndpoint query, CancellationToken ct)
        {
            ITerminal terminal = TimeWarpTerminal.Default;

            string quote = await QuoteHelper.GetRandomQuote();

            terminal.WriteLine();
            terminal.WritePanel(panel => panel
                .Content($"\"{quote}\"".Italic() + "\n\n" + "— Ardalis".Gray())
                .Border(BorderStyle.Rounded)
                .BorderColor(AnsiColors.Cyan)
                .Padding(2, 1));

            return default;
        }
    }
}
