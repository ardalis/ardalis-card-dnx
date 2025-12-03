# Copilot Instructions for ardalis-card-dnx

## Project Overview

A .NET global tool CLI for accessing Ardalis resources built with [TimeWarp.Nuru](https://github.com/TimeWarpEngineering/timewarp-nuru) route-based CLI framework. Runs via `dnx ardalis` (no install) or `ardalis` (after `dotnet tool install -g ardalis`).

## Invariants

- Do NOT change the FrameworkVersion.

## Architecture Patterns

### Route-Based Structure

All commands are defined as routes in `Program.cs` using the fluent `.Map()` API:

- **URL Commands** (open browser): Inline delegates
  ```csharp
  .Map("blog", () => Open(Blog), "Open Ardalis's blog")
  .Map("youtube", () => Open(YouTube), "Open Ardalis's YouTube channel")
  ```

- **Display Commands** (show content): Static handler methods in `Handlers/` folder
  ```csharp
  .Map("card", CardHandler.Execute, "Display Ardalis's business card")
  .Map("quote", async () => await QuoteHandler.ExecuteAsync(), "Display a random quote")
  ```

- **Commands with Options**: Route pattern syntax with typed parameters
  ```csharp
  .Map(
      "packages --all? --page-size? {size:int?}",
      async (bool all, int? size) => await PackagesHandler.ExecuteAsync(all, size ?? 10),
      "Display popular NuGet packages"
  )
  ```

- **Commands with Arguments**: Route pattern with typed parameters
  ```csharp
  .Map(
      "dotnetconf-score {year:int?}",
      async (int? year) => await DotNetConfScoreHandler.ExecuteAsync(year ?? DateTime.Now.Year),
      "Display top .NET Conf videos"
  )
  ```

### Handler Pattern

Handlers are static classes with static methods in the `Handlers/` folder:

```csharp
namespace Ardalis.Cli.Handlers;

public static class MyHandler
{
    public static async Task ExecuteAsync(bool optionFlag, int pageSize)
    {
        ITerminal terminal = NuruTerminal.Default;
        
        // Fetch data with fallback
        List<MyData> data;
        try
        {
            data = await FetchDataAsync();
        }
        catch
        {
            data = [.. FallbackData];
        }
        
        // Display using Nuru terminal API
        terminal.WriteLine("Title".Blue().Bold());
        terminal.WritePanel(panel => panel
            .Content(content)
            .Border(BorderStyle.Rounded)
            .BorderColor(AnsiColors.Cyan));
    }
    
    private static readonly MyData[] FallbackData = [ /* hardcoded fallback */ ];
}
```

### API Fetching with Fallback

**Critical Pattern** - All data-fetching handlers must have fallbacks:
```csharp
try {
    var data = await FetchFromApi();
} catch {
    data = FallbackData; // Hardcoded static readonly array
}
```
Examples: `PackagesHandler.FallbackPackages`, `BooksHandler.FallbackBooks`

### Helper Organization

- `Helpers/UrlHelper.cs` - Opens URLs cross-platform via `Open(url)`
- `Helpers/QuoteHelper.cs` - Fetches random quote from JSON endpoint
- `Helpers/TipHelper.cs` - Fetches random tip from JSON endpoint

### URL Constants

All Ardalis URLs are centralized in `Urls.cs`:
```csharp
using static Ardalis.Cli.Urls;
// Then use: Blog, YouTube, GitHub, etc.
```

## Registration Requirements

### Adding a New Command

1. Create handler in `Handlers/` folder (static class with static method)
2. Add single `.Map()` call in `Program.cs` route chain
3. Update README.md examples

### Route Pattern Syntax

- `command` - Simple literal command
- `command {arg}` - Required argument
- `command {arg?}` - Optional argument
- `command {arg:int}` - Typed argument (int, bool, string, etc.)
- `command --flag` - Required flag
- `command --flag?` - Optional flag
- `command --option {value}` - Option with value

### Version Updates

Update **both** in `ardalis.csproj`:
- `<Version>` - Increment appropriately (major.minor.patch)
- `<ReleaseNotes>` - Describe changes concisely

## Development Workflows

### Testing Locally

```bash
dotnet run -- <command>           # Test specific command
dotnet run -- --help              # Test help output
dotnet run -- -i                  # Test interactive REPL mode
dotnet run -- <command> --verbose # Test verbose output (if applicable)
```

### Publishing

1. Update `<Version>` and `<ReleaseNotes>` in `ardalis.csproj`
2. Commit and push to main
3. Create GitHub Release with tag (e.g., `v1.20.0`) - triggers auto-publish to NuGet
4. See `CONTRIBUTING.md` for details

## TimeWarp.Nuru Terminal API

### Getting the Terminal

```csharp
ITerminal terminal = NuruTerminal.Default;
```

### Text Styling (Fluent API)

```csharp
terminal.WriteLine("Text".Bold());
terminal.WriteLine("Text".Italic());
terminal.WriteLine("Text".Underline());
terminal.WriteLine("Text".Green());
terminal.WriteLine("Text".Cyan().Bold());
terminal.WriteLine("Text".Gray());
```

### Hyperlinks

```csharp
terminal.WriteLine("Click here".Link(url));
terminal.WriteLine("Visit site".Link(url).Cyan());
```

### Panels

```csharp
terminal.WritePanel(panel => panel
    .Content("Panel content here")
    .Header("Panel Title")
    .Border(BorderStyle.Rounded)
    .BorderColor(AnsiColors.Blue)
    .Padding(1, 0));
```

### Tables

```csharp
terminal.WriteTable(table => table
    .AddColumn("Name", col => col.Bold())
    .AddColumn("Value")
    .AddRow("Row 1", "Data 1")
    .AddRow("Row 2", "Data 2")
    .Border(BorderStyle.Rounded));
```

### Colors Available

`AnsiColors.Red`, `AnsiColors.Green`, `AnsiColors.Blue`, `AnsiColors.Yellow`, `AnsiColors.Cyan`, `AnsiColors.Magenta`, `AnsiColors.White`, `AnsiColors.Gray`

### Icons

Use emojis consistently:
- 📝 Blog
- 🎥 YouTube  
- ⚡ GitHub
- 🦋 Bluesky
- 💼 LinkedIn
- 📦 NuGet packages

## Target Framework

Currently targeting **net10.0** (.NET 10 preview). Adjust when stable releases come out.

## Code Style

- Use file-scoped namespaces: `namespace Ardalis.Cli.Handlers;`
- Prefer `var` for local variables
- Use collection expressions: `[item1, item2]`
- Use pattern matching and switch expressions where appropriate
- Static handlers - no instance state or DI in simple handlers
