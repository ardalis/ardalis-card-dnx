# Rewrite Copilot Instructions for Nuru

## Summary

Complete rewrite of `.github/copilot-instructions.md` to document the current TimeWarp.Nuru architecture instead of the deleted Spectre.Console patterns. This is critical as AI assistants use this file to generate code.

## Todo List

- [x] Update Project Overview (line 5) - change Spectre.Console.Cli to TimeWarp.Nuru
- [x] Rewrite Architecture Patterns section (lines 13-41) for route-based handlers
- [x] Update fallback examples with correct class names (lines 43-51)
- [x] Remove RecentHelper reference - was deleted in PR #56 (lines 53-57)
- [x] Rewrite "Adding a New Command" steps for Nuru pattern (lines 67-71)
- [x] Replace Spectre.Console Conventions with Nuru API patterns (lines 94-119)
- [x] Update namespace example from `Ardalis.Commands` to `Ardalis.Cli.Handlers` (line 124)

## Results

Complete rewrite of copilot-instructions.md:
- Updated project overview to reference TimeWarp.Nuru
- Documented route-based architecture with `.Map()` API examples
- Added handler pattern with static classes and methods
- Documented Nuru terminal API (ITerminal, WritePanel, WriteTable, fluent styling)
- Updated "Adding a New Command" to 3-step process
- Added route pattern syntax reference
- Removed all Spectre.Console references and markup syntax
- Updated namespace to `Ardalis.Cli.Handlers`

## Notes

**Current Architecture (TimeWarp.Nuru):**

URL commands use inline delegates:
```csharp
.Map("blog", () => Open(Blog), "Open Ardalis's blog")
```

Display commands use static handler methods:
```csharp
.Map("card", CardHandler.Execute, "Display Ardalis's business card")
.Map("quote", async () => await QuoteHandler.ExecuteAsync(), "...")
```

Commands with options use route pattern syntax:
```csharp
.Map(
    "packages --all? --page-size? {size:int?}",
    async (bool all, int? size) => await PackagesHandler.ExecuteAsync(all, size ?? 10),
    "Display popular Ardalis NuGet packages"
)
```

**Nuru Terminal API:**
```csharp
ITerminal terminal = NuruTerminal.Default;
terminal.WriteLine("Text".Green().Bold());
terminal.WriteLine("Link".Link(url).Cyan());
terminal.WritePanel(panel => panel
    .Content(content)
    .Border(BorderStyle.Rounded)
    .BorderColor(AnsiColors.Blue));
```

**Adding a New Command (Nuru):**
1. Create handler in `Handlers/` folder (static class with static method)
2. Add single `.Map()` call in `Program.cs` route chain
3. Update README.md examples

**Deleted patterns to remove:**
- `Commands/` folder structure
- `Command` and `AsyncCommand<T>` inheritance
- Nested `Settings` class with `[CommandOption]` attributes
- `InteractiveMode.cs` references (Nuru has built-in REPL)
- Spectre.Console markup syntax `[bold green]Text[/]`
- `AnsiConsole.Write()` and `AnsiConsole.MarkupLine()`
