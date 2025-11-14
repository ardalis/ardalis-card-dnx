using Ardalis.Cli.Telemetry;
using Ardalis.Commands;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;
using System;
using System.Threading.Tasks;

namespace Ardalis;

public static class InteractiveMode
{
    public static async Task<int> RunAsync(IServiceProvider services)
    {
        var postHog = services.GetRequiredService<PostHogService>();

        AnsiConsole.MarkupLine("[bold deepskyblue3]Interactive Mode[/]");
        AnsiConsole.MarkupLine("[dim]Enter commands (blog, bluesky, books, card, contact, courses, dometrain, linkedin, nimblepros, packages, pluralsight, quote, recent, repos, speaker, subscribe, tips, youtube). Press Ctrl+C or type 'exit' to quit.[/]\n");

        while (true)
        {
            var input = AnsiConsole.Prompt(
                new TextPrompt<string>("[deepskyblue3]>[/]")
                    .AllowEmpty()
            );

            // Handle exit conditions
            if (string.IsNullOrWhiteSpace(input) ||
                input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.MarkupLine("[dim]Goodbye![/]");
                return 0;
            }

            // Process commands
            var command = input.Trim().ToLowerInvariant();

            try
            {
                switch (command)
                {
                    case "card":
                        services.GetRequiredService<CardCommand>().Execute(null!);
                        break;

                    case "blog":
                        new BlogCommand().Execute(null!);
                        break;

                    case "youtube":
                        new YouTubeCommand().Execute(null!);
                        break;

                    case "bluesky":
                        new BlueSkyCommand().Execute(null!);
                        break;

                    case "linkedin":
                        new LinkedInCommand().Execute(null!);
                        break;

                    case "nimblepros":
                        new NimbleProCommand().Execute(null!);
                        break;

                    case "contact":
                        new ContactCommand().Execute(null!);
                        break;

                    case "dometrain":
                        new DometrainCommand().Execute(null!);
                        break;

                    case "quote":
                        await new QuoteCommand().ExecuteAsync(null!);
                        break;

                    case "repos":
                        await new ReposCommand().ExecuteAsync(null!);
                        break;

                    case "packages":
                        await new PackagesCommand().ExecuteAsync(null!, new PackagesCommand.Settings());
                        break;

                    case "books":
                        await new BooksCommand().ExecuteAsync(null!, new BooksCommand.Settings());
                        break;

                    case "tips":
                        await new TipsCommand().ExecuteAsync(null!);
                        break;

                    case "courses":
                        await new CoursesCommand().ExecuteAsync(null!);
                        break;

                    case "speaker":
                        new SpeakerCommand().Execute(null!);
                        break;

                    case "pluralsight":
                        new PluralsightCommand().Execute(null!);
                        break;

                    case "subscribe":
                        new SubscribeCommand().Execute(null!);
                        break;

                    case "recent":
                        await new RecentCommand().ExecuteAsync(null!, new RecentCommand.Settings());
                        break;

                    case "help":
                    case "?":
                        AnsiConsole.MarkupLine("[bold]Available commands:[/]");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold]Display Commands:[/]");
                        AnsiConsole.MarkupLine("  [deepskyblue3]books[/]   - Display published books");
                        AnsiConsole.MarkupLine("  [deepskyblue3]card[/]    - Display business card");
                        AnsiConsole.MarkupLine("  [deepskyblue3]courses[/] - Display available courses");
                        AnsiConsole.MarkupLine("  [deepskyblue3]packages[/] - Display popular NuGet packages");
                        AnsiConsole.MarkupLine("  [deepskyblue3]quote[/]   - Display random quote");
                        AnsiConsole.MarkupLine("  [deepskyblue3]recent[/]  - Display recent activity");
                        AnsiConsole.MarkupLine("  [deepskyblue3]repos[/]   - Display popular GitHub repositories");
                        AnsiConsole.MarkupLine("  [deepskyblue3]tips[/]    - Display a random coding tip");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold]Open Commands:[/]");
                        AnsiConsole.MarkupLine("  [deepskyblue3]blog[/]    - Open blog");
                        AnsiConsole.MarkupLine("  [deepskyblue3]bluesky[/] - Open Bluesky profile");
                        AnsiConsole.MarkupLine("  [deepskyblue3]contact[/] - Open contact page");
                        AnsiConsole.MarkupLine("  [deepskyblue3]dometrain[/] - Open Dometrain Author profile");
                        AnsiConsole.MarkupLine("  [deepskyblue3]linkedin[/] - Open LinkedIn profile");
                        AnsiConsole.MarkupLine("  [deepskyblue3]nimblepros[/] - Open NimblePros website");
                        AnsiConsole.MarkupLine("  [deepskyblue3]pluralsight[/] - Open Pluralsight profile");
                        AnsiConsole.MarkupLine("  [deepskyblue3]speaker[/] - Open Sessionize speaker profile");
                        AnsiConsole.MarkupLine("  [deepskyblue3]subscribe[/] - Open newsletter subscription page");
                        AnsiConsole.MarkupLine("  [deepskyblue3]youtube[/] - Open YouTube channel");
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("  [deepskyblue3]exit[/]    - Exit interactive mode");
                        break;

                    default:
                        AnsiConsole.MarkupLine($"[red]Unknown command:[/] {input}");
                        AnsiConsole.MarkupLine("[dim]Type 'help' for available commands.[/]");
                        break;
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            }

            AnsiConsole.WriteLine();
        }
    }
}
