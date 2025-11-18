using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;

namespace Ardalis.Commands;

public class CardCommand : Command
{
    public override int Execute(CommandContext context, CancellationToken cancellationToken = default)
    {
        // Top rule with standard color
        var top = new Rule("[deepskyblue3]────────────────────────────────────────[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(top);

        // Card content with UTM tracking
        var ardalisUrl = UrlHelper.AddUtmSource("https://ardalis.com");
        var nimbleprosUrl = UrlHelper.AddUtmSource("https://nimblepros.com");
        var blueskyUrl = UrlHelper.AddUtmSource("https://bsky.app/profile/ardalis.com");
        var linkedinUrl = UrlHelper.AddUtmSource("https://www.linkedin.com/in/stevenandrewsmith/");
        var sessionizeUrl = UrlHelper.AddUtmSource("https://sessionize.com/ardalis");
        
        var panelContent = new Markup(
            "[bold mediumorchid1]Steve 'Ardalis' Smith[/]\n" +
            "[grey]Software Architect, Speaker, and Trainer[/]\n\n" +
            $"[link={ardalisUrl}][deepskyblue3]https://ardalis.com[/][/]\n" +
            $"[link={nimbleprosUrl}][violet]https://nimblepros.com[/][/]\n\n" +
            $"[link={blueskyUrl}][deepskyblue3]BlueSky[/][/] • " +
            $"[link={linkedinUrl}][deepskyblue3]LinkedIn[/][/] • " +
            $"[link={sessionizeUrl}][deepskyblue3]Sessionize[/][/]\n\n" +
            "[italic grey]Clean Architecture • DDD • .NET[/]"
        );

        // Panel with purple border, not full-width
        var panel = new Panel(panelContent)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.MediumOrchid1),
            Padding = new Padding(2, 1, 2, 1),
            Expand = false
        };

        // Simple header (no alignment property on some Spectre versions)
        panel.Header = new PanelHeader("[bold deepskyblue3]💠 Ardalis[/]");

        // Center the whole panel (Spectre.Console centers non-expanded panels by default)
        AnsiConsole.Write(panel);

        // Bottom rule with standard color
        var bottom = new Rule("[mediumorchid1]────────────────────────────────────────[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(bottom);

        AnsiConsole.MarkupLine("\n[dim]Try '[deepskyblue3]ardalis blog[/]' or '[mediumorchid1]ardalis youtube[/]' for more options[/]");

        return 0;
    }
}
