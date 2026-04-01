using System;
using System.Threading.Tasks;
using Ardalis.Cli.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using TimeWarp.Nuru;

namespace Ardalis.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        NuruApp app = NuruApp.CreateBuilder()
            .UseMicrosoftDependencyInjection()
            .ConfigureServices(services =>
            {
                services.AddLogging();

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
            .AddBehavior(typeof(PostHogNuruBehavior))
            // Auto-discover all [NuruRoute] endpoints
            .DiscoverEndpoints()
            // Configure REPL mode
            .AddRepl(options =>
            {
                options.Prompt = "ardalis> ";
                options.WelcomeMessage =
                    "Welcome to Ardalis CLI Interactive Mode!\n" +
                    "Type 'help' for available commands, or 'exit' to quit.";
                options.AutoStartWhenEmpty = true;
            })
            .Build();

        // Check for interactive mode flag
        if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
        {
            await app.RunReplAsync();
            return default;
        }

        await app.RunAsync(args);
        return default;

    }
}
