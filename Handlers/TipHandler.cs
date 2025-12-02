using System.Threading.Tasks;
using Ardalis.Helpers;
using TimeWarp.Nuru;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli.Handlers;

/// <summary>
/// Displays a random coding tip using Nuru terminal.
/// </summary>
public static class TipHandler
{
    public static async Task ExecuteAsync()
    {
        ITerminal terminal = NuruTerminal.Default;

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
    }
}
