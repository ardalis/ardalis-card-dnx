using System.Threading;
using System.Threading.Tasks;
using Ardalis.Helpers;
using TimeWarp.Nuru;
using TimeWarp.Terminal;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays a random coding tip using Nuru terminal.
/// </summary>
[NuruRoute("tip", Description = "Display a random coding tip")]
public sealed class TipEndpoint : IQuery<Unit>
{
    public sealed class Handler : IQueryHandler<TipEndpoint, Unit>
    {
        public async ValueTask<Unit> Handle(TipEndpoint query, CancellationToken ct)
        {
            ITerminal terminal = TimeWarpTerminal.Default;

            TipHelper.Tip tip = await TipHelper.GetRandomTip();

            // Add UTM tracking to URL
            string urlWithTracking = AddUtmSource(tip.ReferenceLink);
            string displayUrl = StripQueryString(tip.ReferenceLink);

            terminal.WriteLine();
            terminal.WritePanel(panel => panel
                .Header("💡 Coding Tip".Yellow().Bold())
                .Content(
                    tip.TipText + "\n\n" +
                    "Learn more: ".Gray() + displayUrl.Link(urlWithTracking).Cyan())
                .Border(BorderStyle.Rounded)
                .BorderColor(AnsiColors.Yellow)
                .Padding(1, 0));

            return default;
        }
    }
}
