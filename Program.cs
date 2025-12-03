using System;
using System.Net.Http;
using System.Threading.Tasks;
using Ardalis.Cli.Handlers;
using Ardalis.Cli.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Nuru;
using static Ardalis.Cli.Urls;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

        NuruAppOptions options = new()
        {
            ConfigureRepl = repl =>
            {
                repl.Prompt = "ardalis> ";
                repl.PromptColor = "\x1b[36m"; // Cyan
                repl.WelcomeMessage =
                    "Welcome to Ardalis CLI Interactive Mode!\n" +
                    "Type 'help' for available commands, or 'exit' to quit.";
                repl.GoodbyeMessage = "Thanks for using Ardalis CLI!";
            }
        };

        NuruCoreApp app = NuruApp.CreateBuilder(args, options)
            .ConfigureServices(services =>
            {
                services.AddSingleton<PostHogService>();
                services.AddMediator();

                // Named HTTP clients for DI
                services.AddHttpClient("GitHub", client =>
                {
                    client.BaseAddress = new Uri("https://api.github.com/");
                    client.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
                    client.Timeout = TimeSpan.FromSeconds(10);
                });

                services.AddHttpClient("NuGet", client =>
                {
                    client.BaseAddress = new Uri("https://api-v2v3search-0.nuget.org/");
                    client.Timeout = TimeSpan.FromSeconds(10);
                });

                services.AddHttpClient("ArdalisApi", client =>
                {
                    client.BaseAddress = new Uri("https://api.ardalis.com/");
                    client.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

                services.AddHttpClient("ArdalisWeb", client =>
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
                    client.Timeout = TimeSpan.FromSeconds(10);
                });

                services.AddHttpClient("RssFeed", client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(10);
                });
            })
            .WithMetadata(
                description: "Ardalis CLI - Tools and links from Steve 'Ardalis' Smith"
            )

            // ========================================
            // URL Commands (open in browser)
            // ========================================
            .Map("blog", () => Open(Blog), "Open Ardalis's blog")
            .Map("bluesky", () => Open(BlueSky), "Open Ardalis's Bluesky profile")
            .Map("contact", () => Open(Contact), "Open Ardalis's contact page")
            .Map("dometrain", () => Open(Dometrain), "Open Ardalis's Dometrain Author profile")
            .Map("github", () => Open(GitHub), "Open Ardalis's GitHub profile")
            .Map("linkedin", () => Open(LinkedIn), "Open Ardalis's LinkedIn profile")
            .Map("nimblepros", () => Open(NimblePros), "Open NimblePros website")
            .Map("nuget", () => Open(NuGet), "Open Ardalis's NuGet profile")
            .Map("pluralsight", () => Open(Pluralsight), "Open Ardalis's Pluralsight profile")
            .Map("speaker", () => Open(Speaker), "Open Ardalis's Sessionize speaker profile")
            .Map("subscribe", () => Open(Subscribe), "Open Ardalis's newsletter subscription page")
            .Map("youtube", () => Open(YouTube), "Open Ardalis's YouTube channel")

            // ========================================
            // Display Commands (show content in terminal)
            // ========================================
            .Map("card", CardHandler.Execute, "Display Ardalis's business card")
            .Map("quote", async () => await QuoteHandler.ExecuteAsync(), "Display a random Ardalis quote")
            .Map("tip", async () => await TipHandler.ExecuteAsync(), "Display a random coding tip")
            .Map<ReposCommand>("repos", "Display popular Ardalis GitHub repositories")

            // ========================================
            // Commands with Options
            // ========================================
            .Map<PackagesCommand>(
                "packages --all? --page-size? {size:int?}",
                "Display popular Ardalis NuGet packages"
            )
            .Map<BooksCommand>(
                "books --no-paging? --page-size? {size:int?}",
                "Display published books by Ardalis"
            )
            .Map<CoursesCommand>(
                "courses --all? --page-size? {size:int?}",
                "Display available courses"
            )
            .Map<RecentCommand>(
                "recent --verbose?",
                "Display recent activity from Ardalis"
            )

            // ========================================
            // Commands with Arguments
            // ========================================
            .Map(
                "dotnetconf-score {year:int?}",
                async (int? year) => await DotNetConfScoreHandler.ExecuteAsync(year ?? DateTime.Now.Year),
                "Display top videos from .NET Conf playlists"
            )

            // ========================================
            // Version Command
            // ========================================
            .Map("version", async () =>
            {
                ITerminal terminal = NuruTerminal.Default;
                terminal.WriteLine(version);

                // Check for updates on NuGet
                try
                {
                    using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                    string response = await httpClient.GetStringAsync("https://api.nuget.org/v3-flatcontainer/ardalis/index.json");
                    var versionData = System.Text.Json.JsonSerializer.Deserialize<NuGetVersionData>(response);

                    if (versionData?.Versions != null && versionData.Versions.Length > 0)
                    {
                        string latestVersion = versionData.Versions[^1];

                        var current = Version.Parse(version);
                        var latest = Version.Parse(latestVersion);

                        terminal.WriteLine();
                        if (latest > current)
                        {
                            terminal.WriteLine($"v{latestVersion} is available; upgrade with:".Yellow());
                            terminal.WriteLine("dotnet tool update -g ardalis".Cyan());
                        }
                        else
                        {
                            terminal.WriteLine("You are on the latest version.".Green());
                        }
                    }
                }
                catch (Exception ex)
                {
                    terminal.WriteLine($"Unable to check for updates: {ex.Message}".Red());
                }
            }, "Display version and check for updates")

            // ========================================
            // Default Handler (no command provided)
            // ========================================
            .MapDefault(() =>
            {
                CardHandler.Execute();
                return 0;
            })

            .Build();

        // Check for interactive mode flag
        if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
        {
            // Note: PostHog tracking for REPL mode is handled by the pipeline behavior
            return await app.RunReplAsync();
        }

        return await app.RunAsync(args);
    }
}
