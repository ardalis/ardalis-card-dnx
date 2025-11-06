using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics;
using System;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("ardalis");
    config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");
    
    config.AddCommand<CardCommand>("card")
        .WithDescription("Display Ardalis's business card.");

    config.AddCommand<BlogCommand>("blog")
        .WithDescription("Open Ardalis's blog.");

    config.AddCommand<YouTubeCommand>("youtube")
        .WithDescription("Open Ardalis's YouTube channel.");
        
    config.AddExample("card");
    config.AddExample("blog");
    config.AddExample("--version");
});

return app.Run(args);

// ===== COMMANDS =====

public class CardCommand : Command
{
    public override int Execute(CommandContext context)
    {
        // Top rule with standard color
        var top = new Rule("[deepskyblue3]────────────────────────────────────────[/]")
        {
            Justification = Justify.Center
        };
        AnsiConsole.Write(top);

        // Card content
        var panelContent = new Markup(
            "[bold mediumorchid1]Steve 'Ardalis' Smith[/]\n" +
            "[grey]Software Architect & Trainer[/]\n\n" +
            "[link=https://ardalis.com][deepskyblue3]https://ardalis.com[/][/]\n" +
            "[link=https://nimblepros.com][violet]https://nimblepros.com[/][/]\n\n" +
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
