# TimeWarp.Nuru Endpoint Migration Plan

**Created**: 2026-04-01  
**Status**: Planning  
**Target Nuru Version**: 3.0.0-beta.68 (from 3.0.0-beta.19)

> ⚠️ **Note**: The local MCP tool (`TimeWarp.Nuru.Mcp`) is on beta.23. Run `dotnet tool update -g TimeWarp.Nuru.Mcp` to get accurate examples for beta.68.

## Overview

Migrate from Fluent DSL (`.Map()` routes) to Endpoint DSL (`[NuruRoute]` attributes) for all CLI commands. This provides:

- Better testability via dependency injection
- Consistent architecture across all commands
- Auto-discovery of routes via `DiscoverEndpoints()`
- Unified pipeline behaviors using `INuruBehavior`
- Removal of Mediator.SourceGenerator dependency (Nuru endpoints have built-in command/query handling)

## Current State

- **Framework**: TimeWarp.Nuru 3.0.0-beta.19
- **Pattern**: Fluent DSL with `.Map()` calls in `Program.cs` (legacy 3-arg syntax)
- **Handler Types**:
  - Static handlers: `CardHandler`, `QuoteHandler`, `TipHandler`
  - Mediator commands: `PackagesCommand`, `BooksCommand`, `CoursesCommand`, `RecentCommand`, `ReposCommand`, `DotNetConfScoreCommand`
  - Inline lambdas: URL opener commands (`blog`, `youtube`, `github`, etc.)
- **Pipeline**: Mediator's `IPipelineBehavior` via `PostHogTrackingBehavior`

## Target State

- **Framework**: TimeWarp.Nuru 3.0.0-beta.68
- **Pattern**: Endpoint DSL with `[NuruRoute]` classes
- **Handler Types**: All endpoints using `IQuery<T>` or `ICommand<Unit>` with nested `Handler` classes
- **Pipeline**: Nuru's `INuruBehavior` for telemetry
- **New Features**: Shell tab completion via `.EnableStaticCompletion()`

## Breaking Changes in beta.68

The Fluent DSL syntax has changed significantly:

**Old syntax (beta.19)**:
```csharp
.Map("blog", () => Open(Blog), "Open Ardalis's blog")
```

**New syntax (beta.68)**:
```csharp
.Map("blog")
  .WithHandler(() => Open(Blog))
  .WithDescription("Open Ardalis's blog")
  .AsQuery()
  .Done()
```

This affects **all** existing `.Map()` calls in `Program.cs`. The migration plan accounts for this by converting directly to Endpoint DSL rather than fixing Fluent syntax first.

---

## Phase 1: Foundation & Infrastructure

**Goal**: Update Nuru package and prepare infrastructure. Handle Fluent API breaking changes.

### 1.1 Update TimeWarp.Nuru Package
- [ ] Update `ardalis.csproj` from `3.0.0-beta.19` to `3.0.0-beta.68`
- [ ] Update MCP tool: `dotnet tool update -g TimeWarp.Nuru.Mcp`
- [ ] Run `dotnet restore` and verify build succeeds
- [ ] **Breaking Change**: Fluent DSL syntax changed - `.Map(route, handler, desc)` must become `.Map(route).WithHandler(handler).WithDescription(desc).AsQuery().Done()` or `.AsCommand().Done()`
- [ ] Test existing commands still work: `dotnet run -- blog`, `dotnet run -- card`

**Note on Breaking Change Strategy**: Rather than fixing all `.Map()` calls to the new Fluent syntax (which would be throwaway work), we'll convert routes directly to Endpoint classes. However, if the build breaks, we may need to temporarily comment out routes or batch the conversion.

### 1.1a Fix Existing Fluent Routes (if build breaks)
- [ ] Convert simple URL routes to new Fluent syntax temporarily:
  ```csharp
  // Old: .Map("blog", () => Open(Blog), "Open Ardalis's blog")
  // New: .Map("blog").WithHandler(() => Open(Blog)).WithDescription("Open Ardalis's blog").AsQuery().Done()
  ```
- [ ] OR: Comment out routes and add them back as endpoints in Phase 2-3

### 1.2 Convert PostHogTrackingBehavior to INuruBehavior
- [ ] Create new `Behaviors/PostHogNuruBehavior.cs` implementing `INuruBehavior`
- [ ] Update `Program.cs` to use `.AddBehavior(typeof(PostHogNuruBehavior))`
- [ ] Keep old `PostHogTrackingBehavior` temporarily for Mediator commands
- [ ] Test telemetry still works

### 1.3 Add DiscoverEndpoints() Infrastructure
- [ ] Add `.DiscoverEndpoints()` call to builder in `Program.cs`
- [ ] Verify existing `.Map()` routes still work alongside endpoint discovery
- [ ] Create first test endpoint to verify discovery works

**Validation**: All existing commands work, telemetry functional, build succeeds.

---

## Phase 2: Convert Static Handlers

**Goal**: Convert simple static handlers to endpoint classes (low risk, no DI dependencies).

### 2.1 Convert CardHandler → CardEndpoint
- [ ] Create `Endpoints/CardEndpoint.cs` with `[NuruRoute("card")]`
- [ ] Implement as `IQuery<Unit>` with nested `Handler`
- [ ] Update `Program.cs`: Remove `.Map("card", ...)` line
- [ ] Update `.MapDefault()` to use new endpoint
- [ ] Test: `dotnet run -- card`

### 2.2 Convert QuoteHandler → QuoteEndpoint
- [ ] Create `Endpoints/QuoteEndpoint.cs` with `[NuruRoute("quote")]`
- [ ] Inject `IHttpClientFactory` for API calls
- [ ] Remove `.Map("quote", ...)` from `Program.cs`
- [ ] Test: `dotnet run -- quote`

### 2.3 Convert TipHandler → TipEndpoint
- [ ] Create `Endpoints/TipEndpoint.cs` with `[NuruRoute("tip")]`
- [ ] Inject `IHttpClientFactory` for API calls
- [ ] Remove `.Map("tip", ...)` from `Program.cs`
- [ ] Test: `dotnet run -- tip`

**Validation**: `card`, `quote`, `tip` commands work with endpoint pattern.

---

## Phase 3: Convert URL Opener Commands

**Goal**: Convert simple browser-opening commands to endpoints (or use `[NuruRouteGroup]` for organization).

### 3.1 Create UrlEndpoints Group
- [ ] Create `Endpoints/UrlEndpoints.cs` with individual endpoint classes
- [ ] Convert each URL command:
  - `blog` → `BlogEndpoint`
  - `bluesky` → `BlueskyEndpoint`
  - `contact` → `ContactEndpoint`
  - `dometrain` → `DometrainEndpoint`
  - `github` → `GithubEndpoint`
  - `linkedin` → `LinkedinEndpoint`
  - `nimblepros` → `NimbleprosEndpoint`
  - `nuget` → `NugetEndpoint`
  - `pluralsight` → `PluralsightEndpoint`
  - `speaker` → `SpeakerEndpoint`
  - `subscribe` → `SubscribeEndpoint`
  - `youtube` → `YoutubeEndpoint`
- [ ] Remove corresponding `.Map()` calls from `Program.cs`

### 3.2 Convert Version Command
- [ ] Create `Endpoints/VersionEndpoint.cs` with `[NuruRoute("version")]`
- [ ] Inject `IHttpClientFactory` for NuGet version check
- [ ] Remove inline lambda from `Program.cs`
- [ ] Test: `dotnet run -- version`

**Validation**: All URL commands and version command work with endpoint pattern.

---

## Phase 4: Convert Mediator Commands to Nuru Endpoints

**Goal**: Replace Mediator-based `IRequest` commands with Nuru's `IQuery<T>`/`ICommand<Unit>`.

### 4.1 Convert ReposCommand → ReposEndpoint
- [ ] Create `Endpoints/ReposEndpoint.cs` with `[NuruRoute("repos")]`
- [ ] Implement as `IQuery<Unit>` with injected `IHttpClientFactory`, `ILogger`
- [ ] Move handler logic from `ReposCommand.Handler` to nested `Handler` class
- [ ] Delete old `Handlers/ReposCommand.cs`
- [ ] Remove `.Map<ReposCommand>(...)` from `Program.cs`
- [ ] Test: `dotnet run -- repos`

### 4.2 Convert PackagesCommand → PackagesEndpoint
- [ ] Create `Endpoints/PackagesEndpoint.cs`
- [ ] Route pattern: `[NuruRoute("packages")]`
- [ ] Add `[Parameter]` properties for `--all` and `--page-size` options
- [ ] Migrate handler logic
- [ ] Delete old `Handlers/PackagesCommand.cs`
- [ ] Test: `dotnet run -- packages`, `dotnet run -- packages --all`

### 4.3 Convert BooksCommand → BooksEndpoint
- [ ] Create `Endpoints/BooksEndpoint.cs`
- [ ] Route pattern: `[NuruRoute("books")]`
- [ ] Add `[Parameter]` for `--no-paging` and `--page-size` options
- [ ] Migrate handler logic
- [ ] Delete old `Handlers/BooksCommand.cs`
- [ ] Test: `dotnet run -- books`

### 4.4 Convert CoursesCommand → CoursesEndpoint
- [ ] Create `Endpoints/CoursesEndpoint.cs`
- [ ] Route pattern: `[NuruRoute("courses")]`
- [ ] Add `[Parameter]` for options
- [ ] Migrate handler logic
- [ ] Delete old `Handlers/CoursesCommand.cs`
- [ ] Test: `dotnet run -- courses`

### 4.5 Convert RecentCommand → RecentEndpoint
- [ ] Create `Endpoints/RecentEndpoint.cs`
- [ ] Route pattern: `[NuruRoute("recent")]`
- [ ] Add `[Parameter]` for `--verbose` option
- [ ] Migrate handler logic
- [ ] Delete old `Handlers/RecentCommand.cs`
- [ ] Test: `dotnet run -- recent`, `dotnet run -- recent --verbose`

### 4.6 Convert DotNetConfScoreCommand → DotNetConfScoreEndpoint
- [ ] Create `Endpoints/DotNetConfScoreEndpoint.cs`
- [ ] Route pattern: `[NuruRoute("dotnetconf-score")]`
- [ ] Add `[Parameter]` for `{year:int?}` argument
- [ ] Migrate handler logic
- [ ] Delete old `Handlers/DotNetConfScoreCommand.cs`
- [ ] Test: `dotnet run -- dotnetconf-score`, `dotnet run -- dotnetconf-score 2024`

**Validation**: All data-fetching commands work with new endpoint pattern.

---

## Phase 5: Cleanup & Finalization

**Goal**: Remove legacy code, update dependencies, and finalize migration.

### 5.1 Remove Mediator Dependencies
- [ ] Delete old `Behaviors/PostHogTrackingBehavior.cs` (Mediator version)
- [ ] Remove `services.AddMediator()` from `Program.cs`
- [ ] Remove from `ardalis.csproj`:
  - `Mediator.Abstractions`
  - `Mediator.SourceGenerator`
- [ ] Run `dotnet restore` and verify build

### 5.2 Reorganize Project Structure
- [ ] Move remaining files from `Handlers/` to `Endpoints/` (or delete if migrated)
- [ ] Delete empty `Handlers/` folder
- [ ] Rename `Behaviors/PostHogNuruBehavior.cs` to `Behaviors/PostHogBehavior.cs`

### 5.3 Clean Up Program.cs
- [ ] Remove all `.Map()` calls (should be empty after endpoint migration)
- [ ] Simplify to: `.DiscoverEndpoints().Build()`
- [ ] Remove unnecessary `using` statements
- [ ] Update `MapDefault()` to call card endpoint

### 5.4 Update Project Metadata
- [ ] Increment version in `ardalis.csproj` (e.g., `1.21.0`)
- [ ] Update `<ReleaseNotes>` with migration summary
- [ ] Update README.md with new architecture overview

### 5.5 Update Documentation
- [ ] Update `copilot-instructions.md` with new Endpoint DSL patterns
- [ ] Update `CONTRIBUTING.md` with new endpoint creation workflow
- [ ] Remove references to old Mediator pattern

**Validation**: Full test pass, no Mediator references, clean build.

---

## Phase 6: Comprehensive Testing

**Goal**: Ensure all functionality works correctly after migration.

### 6.1 Manual Testing Checklist
- [ ] `dotnet run -- --help` shows all commands
- [ ] `dotnet run -- card` displays business card
- [ ] `dotnet run -- quote` displays random quote
- [ ] `dotnet run -- tip` displays random tip
- [ ] `dotnet run -- blog` opens browser
- [ ] `dotnet run -- packages` lists packages
- [ ] `dotnet run -- packages --all` lists all packages
- [ ] `dotnet run -- books` lists books
- [ ] `dotnet run -- courses` lists courses
- [ ] `dotnet run -- repos` lists repositories
- [ ] `dotnet run -- recent` lists recent activity
- [ ] `dotnet run -- recent --verbose` shows verbose output
- [ ] `dotnet run -- dotnetconf-score` shows current year scores
- [ ] `dotnet run -- dotnetconf-score 2024` shows 2024 scores
- [ ] `dotnet run -- version` shows version info
- [ ] `dotnet run -- -i` enters REPL mode
- [ ] Tab completion works

### 6.2 Telemetry Validation
- [ ] Verify PostHog events are captured for all commands
- [ ] Confirm command names are tracked correctly

### 6.3 Error Handling
- [ ] Test fallback data when APIs are unavailable
- [ ] Test invalid arguments show helpful errors

---

## Phase 7: Optional Enhancements (beta.68 New Features)

**Goal**: Add new features available in beta.68.

### 7.1 Enable Shell Tab Completion
- [ ] Add `.EnableStaticCompletion()` to builder in `Program.cs`
- [ ] Test completion generation: `dotnet run -- --generate-completion powershell`
- [ ] Document in README.md how to enable completions
- [ ] Test: Verify `ardalis <TAB>` shows available commands

### 7.2 Consider Native AOT (Future)
- [ ] Evaluate Native AOT build: `dotnet publish -c Release -r win-x64 --self-contained /p:PublishAot=true`
- [ ] Verify all endpoints work with AOT (may require source generator adjustments)
- [ ] Document binary size and startup time improvements

**Validation**: Shell completion works, optional AOT build succeeds.

---

## Endpoint Pattern Reference

### Basic Endpoint (no parameters)
```csharp
[NuruRoute("card", Description = "Display Ardalis's business card")]
public sealed class CardEndpoint : IQuery<Unit>
{
    public sealed class Handler : IQueryHandler<CardEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(CardEndpoint query, CancellationToken ct)
        {
            // Implementation
            return default;
        }
    }
}
```

### Endpoint with Optional Argument
```csharp
[NuruRoute("dotnetconf-score", Description = "Display top .NET Conf videos")]
public sealed class DotNetConfScoreEndpoint : IQuery<Unit>
{
    [Parameter(Description = "Year to display scores for")]
    public int? Year { get; set; }

    public sealed class Handler(IHttpClientFactory httpFactory) 
        : IQueryHandler<DotNetConfScoreEndpoint, Unit>
    {
        public async ValueTask<Unit> Handle(DotNetConfScoreEndpoint query, CancellationToken ct)
        {
            int year = query.Year ?? DateTime.Now.Year;
            // Implementation
            return default;
        }
    }
}
```

### Endpoint with Options (Flags)
```csharp
[NuruRoute("packages", Description = "Display popular NuGet packages")]
public sealed class PackagesEndpoint : IQuery<Unit>
{
    [Parameter(Name = "all", Description = "Show all packages")]
    public bool All { get; set; }

    [Parameter(Name = "page-size", Description = "Number of packages per page")]
    public int? PageSize { get; set; }

    public sealed class Handler(IHttpClientFactory httpFactory, ILogger<Handler> logger) 
        : IQueryHandler<PackagesEndpoint, Unit>
    {
        public async ValueTask<Unit> Handle(PackagesEndpoint query, CancellationToken ct)
        {
            int pageSize = query.PageSize ?? 10;
            // Implementation
            return default;
        }
    }
}
```

### INuruBehavior (Replaces Mediator Pipeline)
```csharp
public sealed class PostHogBehavior : INuruBehavior
{
    private readonly PostHogService _postHog;

    public PostHogBehavior(PostHogService postHog) => _postHog = postHog;

    public async ValueTask HandleAsync(BehaviorContext context, Func<ValueTask> proceed)
    {
        string commandName = context.CommandName;
        _postHog.TrackCommand(commandName);
        await proceed();
    }
}
```

---

## Risk Mitigation

1. **Incremental Migration**: Each phase can be completed and validated independently
2. **Hybrid Support**: Nuru supports both Fluent and Endpoint DSL simultaneously during migration
3. **Direct to Endpoints**: We skip fixing broken Fluent syntax by converting directly to Endpoints (less throwaway work)
4. **Fallback Data**: All data-fetching endpoints have hardcoded fallbacks
5. **Rollback**: Git commits at each phase boundary allow easy rollback
6. **MCP Tool Update**: Updating the MCP tool ensures accurate examples for the target version

## Estimated Effort

| Phase | Description | Estimated Time |
|-------|-------------|----------------|
| 1 | Foundation & Infrastructure | 1-2 hours |
| 2 | Convert Static Handlers | 1 hour |
| 3 | Convert URL Commands | 1-2 hours |
| 4 | Convert Mediator Commands | 3-4 hours |
| 5 | Cleanup & Finalization | 1-2 hours |
| 6 | Comprehensive Testing | 1 hour |
| 7 | Optional Enhancements | 1 hour |
| **Total (Required)** | Phases 1-6 | **8-12 hours** |
| **Total (Full)** | Phases 1-7 | **9-13 hours** |

---

## Next Steps

1. Start with **Phase 1.1**: Update Nuru package version
2. Commit after each sub-phase for easy rollback
3. Mark tasks complete in this plan as you progress
