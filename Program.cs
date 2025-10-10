using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;
using System;

var app = new CommandApp<CardCommand>();

app.Configure(config =>
{
    config.AddCommand<BlogCommand>("blog")
        .WithDescription("Open Ardalis's blog.");

    config.AddCommand<YouTubeCommand>("youtube")
        .WithDescription("Open Ardalis's YouTube channel.");
});

return app.Run(args);

// ===== COMMANDS =====

public class CardCommand : Command
{
    public override int Execute(CommandContext context)
    {
        // Top gradient rule
        var top = new Rule("[gradient(deepskyblue3,mediumorchid1)]────────────────────────────────────────[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(top);

        // Main panel
        var panelContent = new Markup(
            "[bold mediumorchid1]Steve 'Ardalis' Smith[/]\n" +
            "[grey]Software Architect & Trainer[/]\n\n" +
            "[link=https://ardalis.com][deepskyblue3]https://ardalis.com[/][/]\n" +
            "[link=https://nimblepros.com][violet]https://nimblepros.com[/][/]\n\n" +
            "[italic grey]Clean Architecture • DDD • .NET[/]"
        );

        var panel = new Panel(panelContent)
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.MediumOrchid1),
            Padding = new Padding(2, 1, 2, 1),
        };

        // Set header + center alignment on the header object
        panel.Header = new PanelHeader("[bold deepskyblue3]💠 Ardalis[/]")
        {
            Alignment = Justify.Center
        };

        AnsiConsole.Write(panel);

        // Bottom gradient rule
        var bottom = new Rule("[gradient(mediumorchid1,deepskyblue3)]────────────────────────────────────────[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(bottom);

        AnsiConsole.MarkupLine("\n[dim]Try '[deepskyblue3]ardalis blog[/]' or '[mediumorchid1]ardalis youtube[/]'[/]");

        return 0;
    }
}


public class BlogCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var url = "https://ardalis.com";
        AnsiConsole.MarkupLine($"[bold green]Opening blog:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}

public class YouTubeCommand : Command
{
    public override int Execute(CommandContext context)
    {
        var url = "https://youtube.com/@Ardalis";
        AnsiConsole.MarkupLine($"[bold red]Opening YouTube channel:[/] {url}");
        UrlHelper.Open(url);
        return 0;
    }
}

// ===== HELPER =====

public static class UrlHelper
{
    public static void Open(string url)
    {
        try
        {
            using var ps = new Process();
            ps.StartInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            ps.Start();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to open URL:[/] {ex.Message}");
        }
    }
}
