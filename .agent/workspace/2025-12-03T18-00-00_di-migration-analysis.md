# Dependency Injection Migration Analysis for Ardalis CLI

## Executive Summary

This analysis examines implementing **Future Considerations #1 and #2** from the Nuru migration plan: converting delegate routes to Mediator commands with proper DI, and integrating `IHttpClientFactory`. **Recommended approach: Convert all handlers in a single PR** - the changes are mechanical and straightforward.

## Scope

**Analyzed Items:**
- Migration plan items #1 (Delegate Routes vs Mediator Commands) and #2 (IHttpClientFactory Integration)
- GitHub Issue #44: "Configure the app to use services and DI appropriately"
- Current Ardalis CLI handler implementations
- TimeWarp.Nuru Mediator examples

**Goal:** Convert static handlers to Mediator commands with `IHttpClientFactory` via Nuru's `ConfigureServices`.

## Methodology

- Examined all 9 current handlers for dependency patterns
- Reviewed TimeWarp.Nuru mediator and mixed pattern examples

---

## Current State Analysis

### Handler Dependency Audit

| Handler | Static HttpClient | Convert? |
|---------|------------------|----------|
| **DotNetConfScoreHandler** | Yes + `ArdalisApiClient` | Yes |
| **ReposHandler** | Yes | Yes |
| **PackagesHandler** | Yes | Yes |
| **BooksHandler** | Yes | Yes |
| **CoursesHandler** | Yes | Yes |
| **RecentHandler** | Via `RecentHelper` (5 HttpClients!) | Yes |
| **QuoteHandler** | Via `QuoteHelper` | Keep delegate |
| **TipHandler** | Via `TipHelper` | Keep delegate |
| **CardHandler** | None | Keep delegate |

### Current Anti-Patterns

```csharp
// PROBLEM 1: Static HttpClient in every handler (8 instances!)
public static class ReposHandler
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };
    // ...
}

// PROBLEM 2: Everything is static
public static async Task ExecuteAsync() { ... }

// PROBLEM 3: ArdalisApiClient creates its own HttpClient
var client = new ArdalisApiClient(HttpClient, apiKey);
```

**Issues:**
1. **8+ static HttpClient instances** - DNS changes won't be picked up (static clients cache DNS forever)
2. **Scattered configuration** - User-Agent headers set in multiple places

---

## Implementation Plan

### Step 1: Add Named HTTP Clients in ConfigureServices

```csharp
// In Program.cs ConfigureServices
.ConfigureServices(services =>
{
    services.AddSingleton<PostHogService>();
    
    services.AddMediator(opts =>
    {
        opts.PipelineBehaviors =
        [
            typeof(PostHogTrackingBehavior<,>)
        ];
    });
    
    // Named HTTP clients
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
```

### Step 2: Convert Handlers to Mediator Commands

#### Example: ReposCommand

```csharp
// Handlers/ReposCommand.cs
using Mediator;
using TimeWarp.Nuru;

namespace Ardalis.Cli.Handlers;

public sealed class ReposCommand : IRequest
{
    public sealed class Handler : IRequestHandler<ReposCommand>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Handler> _logger;
        
        private static readonly string[] RepoNames =
        [
            "CleanArchitecture",
            "Specification",
            "GuardClauses",
            "Result",
            "SmartEnum"
        ];
        
        public Handler(
            IHttpClientFactory httpClientFactory,
            ILogger<Handler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        
        public async ValueTask<Unit> Handle(
            ReposCommand request, 
            CancellationToken ct)
        {
            ITerminal terminal = NuruTerminal.Default;
            
            terminal.WriteLine("Ardalis's Popular GitHub Repositories".Green().Bold());
            terminal.WriteLine();
            
            var client = _httpClientFactory.CreateClient("GitHub");
            var repos = new List<(string Name, GitHubRepo Info)>();
            
            foreach (var repoName in RepoNames)
            {
                try
                {
                    var response = await client.GetAsync(
                        $"repos/ardalis/{repoName}", ct);
                    response.EnsureSuccessStatusCode();
                    
                    var repoInfo = await response.Content
                        .ReadFromJsonAsync<GitHubRepo>(ct);
                    
                    if (repoInfo != null)
                        repos.Add((repoName, repoInfo));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, 
                        "Failed to fetch repo {RepoName}", repoName);
                }
            }
            
            repos.Sort((a, b) => 
                b.Info.StargazersCount.CompareTo(a.Info.StargazersCount));
            
            DisplayTable(terminal, repos);
            
            return Unit.Value;
        }
        
        private static void DisplayTable(ITerminal terminal, List<(string Name, GitHubRepo Info)> repos)
        {
            var table = new Table()
                .AddColumn("Repository")
                .AddColumn("Stars", Alignment.Right)
                .AddColumn("Description");
            
            table.Border = BorderStyle.Rounded;
            
            foreach (var (repoName, repoInfo) in repos)
            {
                var description = repoInfo.Description ?? "No description";
                if (description.Length > 60)
                    description = description[..57] + "...";
                
                table.AddRow(
                    repoName.Link(repoInfo.HtmlUrl).Cyan(),
                    $"⭐ {repoInfo.StargazersCount:N0}".Yellow(),
                    description.Gray()
                );
            }
            
            terminal.WriteTable(table);
            terminal.WriteLine();
            terminal.WriteLine("Visit: ".Gray() + 
                Urls.GitHub.Link(Urls.GitHub).Cyan());
        }
    }
    
    private sealed class GitHubRepo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        
        [JsonPropertyName("description")]
        public string? Description { get; set; }
        
        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }
        
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = "";
    }
}
```

#### Program.cs Route Registration

```csharp
// Replace delegate with Mediator command
.Map<ReposCommand>("repos", "Display popular Ardalis GitHub repositories")
```

### Step 3: Update All Routes in Program.cs

```csharp
// Display commands - convert to Mediator
.Map<ReposCommand>("repos", "Display popular Ardalis GitHub repositories")
.Map<PackagesCommand>("packages --all? --page-size? {size:int?}", "Display popular Ardalis NuGet packages")
.Map<BooksCommand>("books --no-paging? --page-size? {size:int?}", "Display published books by Ardalis")
.Map<CoursesCommand>("courses --all? --page-size? {size:int?}", "Display available courses")
.Map<RecentCommand>("recent --verbose?", "Display recent activity from Ardalis")
.Map<DotNetConfScoreCommand>("dotnetconf-score {year:int?}", "Display top videos from .NET Conf playlists")

// Keep as delegates - no dependencies
.Map("card", CardHandler.Execute, "Display Ardalis's business card")
.Map("quote", async () => await QuoteHandler.ExecuteAsync(), "Display a random Ardalis quote")
.Map("tip", async () => await TipHandler.ExecuteAsync(), "Display a random coding tip")
```

---

## Handlers to Convert

| Handler | Named Client | Parameters |
|---------|-------------|------------|
| `ReposCommand` | GitHub | None |
| `PackagesCommand` | NuGet | `bool all`, `int? size` |
| `BooksCommand` | ArdalisWeb | `bool noPaging`, `int? size` |
| `CoursesCommand` | ArdalisWeb | `bool all`, `int? size` |
| `RecentCommand` | RssFeed | `bool verbose` |
| `DotNetConfScoreCommand` | ArdalisApi, ArdalisWeb | `int? year` |

## Handlers to Keep as Delegates

| Handler | Rationale |
|---------|-----------|
| `CardHandler` | No dependencies, pure display logic |
| `QuoteHandler` | Simple, uses QuoteHelper |
| `TipHandler` | Simple, uses TipHelper |
| URL commands | Zero dependencies, inline lambdas |

---

## File Changes Summary

### New/Modified Files

| File | Change |
|------|--------|
| `Program.cs` | Add HttpClient registrations, update route mappings |
| `Handlers/ReposCommand.cs` | New Mediator command |
| `Handlers/PackagesCommand.cs` | New Mediator command |
| `Handlers/BooksCommand.cs` | New Mediator command |
| `Handlers/CoursesCommand.cs` | New Mediator command |
| `Handlers/RecentCommand.cs` | New Mediator command |
| `Handlers/DotNetConfScoreCommand.cs` | New Mediator command |

### Files to Delete

| File | Replaced By |
|------|-------------|
| `Handlers/ReposHandler.cs` | `Handlers/ReposCommand.cs` |
| `Handlers/PackagesHandler.cs` | `Handlers/PackagesCommand.cs` |
| `Handlers/BooksHandler.cs` | `Handlers/BooksCommand.cs` |
| `Handlers/CoursesHandler.cs` | `Handlers/CoursesCommand.cs` |
| `Handlers/RecentHandler.cs` | `Handlers/RecentCommand.cs` |
| `Handlers/DotNetConfScoreHandler.cs` | `Handlers/DotNetConfScoreCommand.cs` |
| `Helpers/RecentHelper.cs` | Functionality moved into `RecentCommand` |

---

## References

- [TimeWarp.Nuru Mediator Example](https://github.com/TimeWarpEngineering/timewarp-nuru/samples/calculator/calc-mediator.cs)
- [IHttpClientFactory Documentation](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)

---

## Conclusion

The migration from static delegate handlers to Mediator commands with `IHttpClientFactory` is straightforward:

1. **6 handlers to convert** - mechanical transformation
2. **3 handlers stay as delegates** - no changes needed
3. **Add named HTTP clients** - centralized configuration in `ConfigureServices`

**Approach: Single PR** - the changes follow a consistent pattern and are easy to review together.
