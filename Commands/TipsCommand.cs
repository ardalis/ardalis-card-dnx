using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class TipsCommand : AsyncCommand
{
    private readonly PostHogService _postHog;

    public TipsCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("tips");
        var tip = await TipHelper.GetRandomTip();

        // Add UTM tracking to the URL but display without query string
        var urlWithTracking = UrlHelper.AddUtmSource(tip.ReferenceLink);
        var displayUrl = UrlHelper.StripQueryString(tip.ReferenceLink);

        var panel = new Panel(new Markup(
            $"[bold yellow]💡 Coding Tip[/]\n\n" +
            $"{tip.TipText}\n\n" +
            $"[dim]Learn more:[/] [link={urlWithTracking}]{displayUrl}[/]"
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
