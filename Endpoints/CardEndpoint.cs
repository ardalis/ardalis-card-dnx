using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using TimeWarp.Terminal;
using static Ardalis.Cli.Urls;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays Ardalis business card using Nuru terminal widgets.
/// </summary>
[NuruRoute("card", Description = "Display Ardalis's business card")]
public sealed class CardEndpoint : IQuery<Unit>
{
    public sealed class Handler : IQueryHandler<CardEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(CardEndpoint query, CancellationToken ct)
        {
            ITerminal terminal = TimeWarpTerminal.Default;

            // URLs with UTM tracking
            string ardalisUrl = AddUtmSource(Blog);
            string nimbleprosUrl = AddUtmSource(NimblePros);
            string blueskyUrl = AddUtmSource(BlueSky);
            string linkedinUrl = AddUtmSource(LinkedIn);
            string sessionizeUrl = AddUtmSource(Speaker);

            // Top rule
            terminal.WriteRule(rule => rule.Color(AnsiColors.Cyan));

            // Card panel
            terminal.WritePanel(panel => panel
                .Header("💠 Ardalis".Cyan().Bold())
                .Content(
                    "Steve 'Ardalis' Smith".Magenta().Bold() + "\n" +
                    "Software Architect, Speaker, and Trainer".Gray() + "\n\n" +
                    Blog.Link(ardalisUrl).Cyan() + "\n" +
                    NimblePros.Link(nimbleprosUrl).Magenta() + "\n\n" +
                    "BlueSky".Link(blueskyUrl).Cyan() + " • " +
                    "LinkedIn".Link(linkedinUrl).Cyan() + " • " +
                    "Sessionize".Link(sessionizeUrl).Cyan() + "\n\n" +
                    "Clean Architecture • DDD • .NET".Gray().Italic())
                .Border(BorderStyle.Rounded)
                .BorderColor(AnsiColors.Magenta)
                .Padding(2, 1));

            // Bottom rule
            terminal.WriteRule(rule => rule.Color(AnsiColors.Magenta));

            terminal.WriteLine();
            terminal.WriteLine(
                "Try '".Gray() +
                "ardalis blog".Cyan() +
                "' or '".Gray() +
                "ardalis youtube".Magenta() +
                "' for more options".Gray());

            return default;
        }
    }
}
