using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Ardalis;
using Ardalis.Cli.Infrastructure;
using Ardalis.Cli.Telemetry;
using Ardalis.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ardalis.Cli;

public class Program
{
    private static IHost BuildApplicationAsync(string[] args)
    {
        // Check for interactive mode
        bool interactive = args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive");


        var settings = new HostApplicationBuilderSettings
        {
            Configuration = new ConfigurationManager()
        };
        settings.Configuration.AddEnvironmentVariables();

#if DEBUG
        // Add in-memory configuration for debugging
        settings.Configuration.AddInMemoryCollection(new Dictionary<string, string>
        {
            ["DetailedErrors"] = "true"
        });
#endif

        var builder = Host.CreateEmptyApplicationBuilder(settings);

        // Always configure OpenTelemetry.
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

#if DEBUG
        // source: aspire cli https://github.com/dotnet/aspire/blob/main/src/Aspire.Cli/Program.cs
        var otelBuilder = builder.Services
                    .AddOpenTelemetry()
                    .WithTracing(tracing =>
                    {
                        tracing.AddSource(ArdalisCliTelemetry.ActivitySourceName);

                        tracing.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("ardalis-cli"));
                    });

        // Support both generic OTLP endpoint and PostHog-specific endpoint
        var otlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
                          ?? builder.Configuration["POSTHOG_OTLP_ENDPOINT"];

        if (otlpEndpoint is { })
        {
            // NOTE: If we always enable the OTEL exporter it dramatically
            //       impacts the CLI in terms of exiting quickly because it
            //       has to finish sending telemetry.
            otelBuilder.UseOtlpExporter();

            // For PostHog, you'll need to set:
            // POSTHOG_OTLP_ENDPOINT=https://us.i.posthog.com/v1/traces (or your region)
            // OTEL_EXPORTER_OTLP_HEADERS=x-posthog-auth=<your-project-api-key>
        }
#endif


        var debugMode = args?.Any(a => a == "--debug" || a == "-d") ?? false;

        if (debugMode)
        {
            builder.Logging.AddFilter("Ardalis.Cli", LogLevel.Debug);
            builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning); // Reduce noise from hosting lifecycle
            builder.Logging.AddFilter("PostHog", LogLevel.Trace);

            // Add simple console logging for debug mode
            builder.Logging.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = true;
                options.TimestampFormat = "HH:mm:ss ";
            });
        }

        // add services
        builder.Services.AddSingleton<ArdalisCliTelemetry>();
        //builder.Services.AddPostHog();
        builder.Services.AddSingleton<PostHogService>();

        // Register commands that need DI
        builder.Services.AddTransient<CardCommand>();

        var app = builder.Build();
        return app;
    }

    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        using var host = BuildApplicationAsync(args);
        await host.StartAsync().ConfigureAwait(false);

        // Get PostHog service for tracking
        var posthog = host.Services.GetRequiredService<PostHogService>();

        // Check for interactive mode
        if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
        {
            posthog.TrackInteractiveMode(started: true);
            var result = await InteractiveMode.RunAsync(host.Services);
            posthog.TrackInteractiveMode(started: false);
            return result;
        }

        // Check for version flag to add update notification
        if (args.Length > 0 && (args[0] == "-v" || args[0] == "--version" || args[0] == "version"))
        {
            var currentVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";
            AnsiConsole.WriteLine(currentVersion);

            // Check for updates on NuGet
            try
            {
                using var httpClient = new System.Net.Http.HttpClient { Timeout = System.TimeSpan.FromSeconds(3) };
                var response = await httpClient.GetStringAsync("https://api.nuget.org/v3-flatcontainer/ardalis/index.json");
                var versionData = System.Text.Json.JsonSerializer.Deserialize<NuGetVersionData>(response);

                if (versionData?.Versions != null && versionData.Versions.Length > 0)
                {
                    var latestVersion = versionData.Versions[^1]; // Get last version (latest)

                    // Parse versions for comparison
                    var current = System.Version.Parse(currentVersion);
                    var latest = System.Version.Parse(latestVersion);

                    AnsiConsole.WriteLine();
                    if (latest > current)
                    {
                        AnsiConsole.MarkupLine($"[yellow]v{latestVersion} is available; upgrade with:[/]");
                        AnsiConsole.MarkupLine($"[cyan]dotnet tool update -g ardalis[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]You are on the latest version.[/]");
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: Use logging and telemetry to report errors
                AnsiConsole.MarkupLine($"[red]Unable to check for updates: {ex.Message}[/]");
            }

            return 0;
        }

        // Check for help flag to add custom installation instructions
        if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "help"))
        {
            var helpApp = new CommandApp();
            helpApp.Configure(config =>
            {
                config.SetApplicationName("ardalis");
                config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");

                // Display commands (alphabetical)
                config.AddCommand<BooksCommand>("books").WithDescription("Display published books by Ardalis.");
                config.AddCommand<CardCommand>("card").WithDescription("Display Ardalis's business card.");
                config.AddCommand<CoursesCommand>("courses").WithDescription("Display available courses.");
                config.AddCommand<PackagesCommand>("packages").WithDescription("Display popular Ardalis NuGet packages.");
                config.AddCommand<QuoteCommand>("quote").WithDescription("Display a random Ardalis quote.");
                config.AddCommand<RecentCommand>("recent").WithDescription("Display recent activity from Ardalis.");
                config.AddCommand<ReposCommand>("repos").WithDescription("Display popular Ardalis GitHub repositories.");
                config.AddCommand<TipsCommand>("tips").WithDescription("Display a random coding tip.");

                // Open commands (alphabetical)
                config.AddCommand<BlogCommand>("blog").WithDescription("Open Ardalis's blog.");
                config.AddCommand<BlueSkyCommand>("bluesky").WithDescription("Open Ardalis's Bluesky profile.");
                config.AddCommand<ContactCommand>("contact").WithDescription("Open Ardalis's contact page.");
                config.AddCommand<DometrainCommand>("dometrain").WithDescription("Open Ardalis's Dometrain Author profile.");
                config.AddCommand<LinkedInCommand>("linkedin").WithDescription("Open Ardalis's LinkedIn profile.");
                config.AddCommand<PluralsightCommand>("pluralsight").WithDescription("Open Ardalis's Pluralsight profile.");
                config.AddCommand<NimbleProCommand>("nimblepros").WithDescription("Open NimblePros website.");
                config.AddCommand<SpeakerCommand>("speaker").WithDescription("Open Ardalis's Sessionize speaker profile.");
                config.AddCommand<SubscribeCommand>("subscribe").WithDescription("Open Ardalis's newsletter subscription page.");
                config.AddCommand<YouTubeCommand>("youtube").WithDescription("Open Ardalis's YouTube channel.");

                config.AddExample("card");
                config.AddExample("blog");
                config.AddExample("-i");
            });

            var result = helpApp.Run(args);

            // Add installation instructions after the help output
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold]INSTALLATION:[/]");
            AnsiConsole.MarkupLine("  Install as a global .NET tool:");
            AnsiConsole.MarkupLine("    [cyan]dotnet tool install -g ardalis[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  Update to the latest version:");
            AnsiConsole.MarkupLine("    [cyan]dotnet tool update -g ardalis[/]");
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  [dim]Once installed, use 'ardalis' instead of 'dnx ardalis'[/]");

            return result;
        }

        // Create CommandApp with DI support
        var app = new CommandApp(new TypeRegistrar(host.Services));

        app.Configure(config =>
        {
            config.SetApplicationName("ardalis");
            config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");

            // Display commands (alphabetical)
            config.AddCommand<BooksCommand>("books")
                .WithDescription("Display published books by Ardalis.");

            config.AddCommand<CardCommand>("card")
                .WithDescription("Display Ardalis's business card.");

            config.AddCommand<CoursesCommand>("courses")
                .WithDescription("Display available courses.");

            config.AddCommand<PackagesCommand>("packages")
                .WithDescription("Display popular Ardalis NuGet packages.");

            config.AddCommand<QuoteCommand>("quote")
                .WithDescription("Display a random Ardalis quote.");

            config.AddCommand<RecentCommand>("recent")
                .WithDescription("Display recent activity from Ardalis.");

            config.AddCommand<ReposCommand>("repos")
                .WithDescription("Display popular Ardalis GitHub repositories.");

            config.AddCommand<TipsCommand>("tips")
                .WithDescription("Display a random coding tip.");

            // Open commands (alphabetical)
            config.AddCommand<BlogCommand>("blog")
                .WithDescription("Open Ardalis's blog.");

            config.AddCommand<BlueSkyCommand>("bluesky")
                .WithDescription("Open Ardalis's Bluesky profile.");

            config.AddCommand<ContactCommand>("contact")
                .WithDescription("Open Ardalis's contact page.");

            config.AddCommand<DometrainCommand>("dometrain")
                .WithDescription("Open Ardalis's Dometrain Author profile.");

            config.AddCommand<LinkedInCommand>("linkedin")
                .WithDescription("Open Ardalis's LinkedIn profile.");

            config.AddCommand<PluralsightCommand>("pluralsight")
                .WithDescription("Open Ardalis's Pluralsight profile.");
            config.AddCommand<NimbleProCommand>("nimblepros")
                .WithDescription("Open NimblePros website.");

            config.AddCommand<SpeakerCommand>("speaker")
                .WithDescription("Open Ardalis's Sessionize speaker profile.");

            config.AddCommand<SubscribeCommand>("subscribe")
                .WithDescription("Open Ardalis's newsletter subscription page.");

            config.AddCommand<YouTubeCommand>("youtube")
                .WithDescription("Open Ardalis's YouTube channel.");

            config.AddExample("card");
            config.AddExample("blog");
            config.AddExample("-i");
        });

        // Filter out debug flags before passing to Spectre.Console
        var filteredArgs = args.Where(a => a != "--debug" && a != "-d").ToArray();

        // Track when no command is provided (shows help)
        if (filteredArgs.Length == 0)
        {
            posthog.TrackCommand("(none)");
        }

        var exitCode = await app.RunAsync(filteredArgs);

        // Give PostHog a moment to flush events in background
        // The Dispose() starts the flush asynchronously, this delay allows it to complete
        await Task.Delay(200);

        return exitCode;
    }
}
