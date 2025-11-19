using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class PackagesCommand : AsyncCommand<PackagesCommand.Settings>
{
    private readonly PostHogService _postHog;

    public PackagesCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("--all")]
        [Description("Show all packages without paging")]
        public bool ShowAll { get; set; }

        [CommandOption("--page-size")]
        [Description("Sets page size (default: 10)")]
        public int PageSize { get; set; } = 10;
    }
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    // Fallback list in case API is unavailable
    private static readonly PackageInfo[] FallbackPackages = new[]
    {
        new PackageInfo("Ardalis.GuardClauses", "Guard clause extensions for validating method arguments", 36255041),
        new PackageInfo("Ardalis.Specification", "Base classes for implementing the Specification pattern", 13083928),
        new PackageInfo("Ardalis.Result", "A result abstraction for modeling success/failure without exceptions", 6326436),
        new PackageInfo("Ardalis.ApiEndpoints", "A library for organizing API endpoints using MediatR-like handlers", 4437628),
        new PackageInfo("Ardalis.SmartEnum", "A type-safe, extensible alternative to enums", 19914393),
        new PackageInfo("Ardalis.HttpClientTestExtensions", "Test helpers for testing code that uses HttpClient", 1062914),
        new PackageInfo("Ardalis.SharedKernel", "Base types for Domain-Driven Design and Clean Architecture", 414411)
    };

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("packages");
        AnsiConsole.MarkupLine("[bold green]Ardalis's Popular NuGet Packages[/]\n");

        // Try to fetch packages from NuGet API, fall back to hardcoded list if it fails
        var packages = await GetPackagesFromApi(settings.ShowAll);
        if (packages == null || packages.Length == 0)
        {
            packages = FallbackPackages;
        }

        if (settings.ShowAll)
        {
            // Display all packages in a single table
            DisplayPackagesTable(packages);
        }
        else
        {
            // Display packages with paging - each page is a separate table
            var packagesList = packages.ToList();
            var currentIndex = 0;

            while (currentIndex < packagesList.Count)
            {
                var endIndex = Math.Min(currentIndex + settings.PageSize, packagesList.Count);
                var pagePackages = packagesList.Skip(currentIndex).Take(endIndex - currentIndex).ToArray();
                
                DisplayPackagesTable(pagePackages);
                
                currentIndex = endIndex;

                // Check if there are more items to display
                if (currentIndex < packagesList.Count)
                {
                    AnsiConsole.WriteLine();
                    AnsiConsole.Markup("[dim]Press [bold]Space[/] for more, or any other key to exit...[/]");
                    
                    var key = Console.ReadKey(intercept: true);
                    AnsiConsole.WriteLine(); // Clear the prompt line
                    
                    if (key.Key != ConsoleKey.Spacebar)
                    {
                        AnsiConsole.MarkupLine("[dim]Showing {0} of {1} packages[/]", currentIndex, packagesList.Count);
                        break;
                    }
                    AnsiConsole.WriteLine(); // Add spacing between pages
                }
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Visit: [link]https://www.nuget.org/profiles/ardalis[/][/]");

        return 0;
    }

    private static void DisplayPackagesTable(PackageInfo[] packages)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[bold]Package[/]");
        table.AddColumn("[bold]Downloads[/]");
        table.AddColumn("[bold]Description[/]");

        foreach (var package in packages)
        {
            try
            {
                var downloads = package.TotalDownloads.ToString("N0");
                var description = package.Description;

                // Truncate description if too long
                if (description.Length > 50)
                {
                    description = description.Substring(0, 47) + "...";
                }

                var nugetUrl = $"https://www.nuget.org/packages/{package.Name}";
                var urlWithTracking = UrlHelper.AddUtmSource(nugetUrl);
                table.AddRow(
                    $"[link={urlWithTracking}]{package.Name}[/]",
                    $"[yellow]📦 {downloads}[/]",
                    $"[dim]{description}[/]"
                );
            }
            catch
            {
                // Skip packages that fail to load
                var nugetUrl = $"https://www.nuget.org/packages/{package.Name}";
                var urlWithTracking = UrlHelper.AddUtmSource(nugetUrl);
                table.AddRow(
                    $"[link={urlWithTracking}]{package.Name}[/]",
                    "[dim]N/A[/]",
                    "[dim]Failed to load stats[/]"
                );
            }
        }

        AnsiConsole.Write(table);
    }

    private static async Task<PackageInfo[]> GetPackagesFromApi(bool showAll)
    {
        try
        {
            var url = "https://api-v2v3search-0.nuget.org/query?q=owner:ardalis&take=100";
            var response = await _httpClient.GetStringAsync(url);
            var searchResult = JsonSerializer.Deserialize<NuGetSearchResult>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (searchResult == null || searchResult.Data == null)
            {
                return null;
            }

            var packages = searchResult.Data.AsEnumerable();

            // Filter packages: only include those with 0 or 1 periods (exclude sub-packages like Ardalis.Result.AspNetCore)
            if (!showAll)
            {
                packages = packages.Where(p => p.Id.Count(c => c == '.') <= 1);
            }

            var filteredPackages = packages
                .OrderByDescending(p => p.TotalDownloads)
                .Select(p => new PackageInfo(p.Id, p.Description, p.TotalDownloads))
                .ToArray();

            return filteredPackages;
        }
        catch
        {
            // Return null to trigger fallback
            return null;
        }
    }

    private record PackageInfo(string Name, string Description, long TotalDownloads = 0);

    private class NuGetSearchResult
    {
        public NuGetPackageData[] Data { get; set; }
    }

    private class NuGetPackageData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("totalDownloads")]
        public long TotalDownloads { get; set; }
    }
}
