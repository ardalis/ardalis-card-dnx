# Update Documentation Post-Nuru Migration

## Summary

Update all documentation to reflect the Spectre.Console to TimeWarp.Nuru migration completed in PR #53. Documentation still references obsolete patterns, deleted files, and removed features.

## Todo List

### Critical (README.md)

- [x] Remove all `--with-covers` references (lines 102-103, 259) - feature was removed
- [x] Update framework attribution from Spectre.Console to TimeWarp.Nuru (line 288-289)
- [x] Add missing `github` command to command list
- [x] Add missing `nuget` command to command list
- [x] Document `--interactive` flag alongside `-i` for REPL mode

### Critical (copilot-instructions.md) - Complete Rewrite

- [x] Update project overview to reference TimeWarp.Nuru
- [x] Replace Command class structure with route-based pattern documentation
- [x] Remove obsolete `AsyncCommand<T>` and `CommandSettings` patterns
- [x] Document Nuru route pattern syntax (`{param}`, `{param?}`, `--flag?`, etc.)
- [x] Document Handler pattern (static classes in `Handlers/`)
- [x] Document URL constants pattern (`Urls.cs` + inline lambdas)
- [x] Remove obsolete registration instructions (no Commands folder, no InteractiveMode.cs)
- [x] Replace Spectre.Console markup conventions with Nuru fluent extensions
- [x] Document pipeline behaviors for cross-cutting concerns
- [x] Add current project structure

### Medium (CONTRIBUTING.md)

- [x] Update `QuoteCommand` reference to `QuoteHandler`
- [x] Update `TipsCommand` reference to `TipHandler`
- [x] Update `CoursesCommand` reference to `CoursesHandler`
- [x] Update `BooksCommand` reference to `BooksHandler`

## Notes

### Files to Update

1. **README.md** - User-facing documentation
2. **CONTRIBUTING.md** - Contributor guidelines
3. **.github/copilot-instructions.md** - AI assistant instructions (needs complete rewrite)

### Key Architecture Changes to Document

**Old (Spectre.Console.Cli):**
- Command classes inheriting from `Command` or `AsyncCommand<T>`
- `CommandSettings` classes with `[CommandOption]` attributes
- TypeRegistrar/TypeResolver DI bridge
- Custom `InteractiveMode.cs` for REPL
- Spectre markup: `[bold green]Text[/]`, `[link=url]Click[/]`

**New (TimeWarp.Nuru):**
- Route-based patterns: `.Map("command --flag? {param:int?}", handler, description)`
- Static handler classes in `Handlers/` directory
- URL constants in `Urls.cs`
- Built-in REPL via `app.RunReplAsync()`
- Nuru fluent extensions: `"Text".Green().Bold()`, `"Click".Link(url)`
- Pipeline behaviors in `Behaviors/` for cross-cutting concerns

### Current Project Structure

```
ardalis/
├── Behaviors/
│   └── PostHogTrackingBehavior.cs
├── Handlers/
│   ├── BooksHandler.cs
│   ├── CardHandler.cs
│   ├── CoursesHandler.cs
│   ├── DotNetConfScoreHandler.cs
│   ├── PackagesHandler.cs
│   ├── QuoteHandler.cs
│   ├── RecentHandler.cs
│   ├── ReposHandler.cs
│   └── TipHandler.cs
├── Helpers/
│   ├── QuoteHelper.cs
│   ├── RecentHelper.cs
│   ├── TipHelper.cs
│   └── UrlHelper.cs
├── Telemetry/
│   ├── ArdalisCliTelemetry.cs
│   ├── LoggingHttpClientFactory.cs
│   └── PostHogService.cs
├── Program.cs
├── Urls.cs
├── ArdalisApiClient.cs
└── ardalis.csproj
```

### Reference Documents

- Full analysis: `.agent/workspace/2025-12-03T12-00-00_documentation-review-nuru-migration.md`
- Migration plan: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md`
- Merge PR: https://github.com/ardalis/ardalis-card-dnx/pull/53
