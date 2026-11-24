# Issues #80 and #81: Node 24 GitHub Actions + interactive `help`

- https://github.com/ardalis/ardalis-card-dnx/issues/80: Update GitHub Actions off deprecated Node.js 20
- https://github.com/ardalis/ardalis-card-dnx/issues/81: Interactive Help Fails

Both ship in a single PR (user request).

## Findings

- **#81**: The Nuru REPL sends each line to the same generated router as the command line.
  That router only has a built-in `--help`/`-h`. The REPL's own built-ins are only
  `exit`/`quit`/`q`, `clear`/`cls` and `history`, so `help` falls through to
  "Unknown command. Use --help for usage." The welcome message in `Program.cs` tells users to
  type `help`.
- **Related bug found while reproducing**: `ardalis -i` / `ardalis --interactive` crashes
  with `InvalidOperationException: RunReplAsync was not intercepted`. `Program.cs` calls
  `app.RunReplAsync()` directly, but the source generator doesn't intercept that call site.
  The generated `RunAsync` already handles `-i`/`--interactive`, so the manual branch is
  redundant and broken.

## Phase 1: GitHub Actions (#80)

- [x] `build.yml`: `actions/checkout@v4` → `@v6`, `actions/setup-dotnet@v4` → `@v5`,
      `actions/upload-artifact@v4` → `@v5` (2 steps). See the note on versions below.
- [x] `publish.yml`: `actions/checkout@v4` → `@v6`, `actions/setup-dotnet@v4` → `@v5`.
- [x] Check release notes for breaking changes (upload-artifact hidden files / artifact naming;
      checkout credentials persistence; setup-dotnet cache defaults).

## Phase 2: Interactive `help` (#81)

- [x] Add `HelpEndpoint` (`[NuruRoute("help")]`). It reflects over `[NuruRoute]` and
      `[NuruRouteAlias]` attributes in the CLI assembly to list the commands, so the list
      can't drift from the real routes.
- [x] Also list the interactive built-ins (`exit`/`quit`, `clear`/`cls`, `history`) and point
      to `<command> --help`.
- [x] Remove the broken manual `-i`/`--interactive` branch in `Program.cs`; rely on the
      generated handling.

## Automated tests

- [x] `tests/Ardalis.Cli.Tests/Endpoints/HelpEndpointTests/HandlerTests.cs` (TUnit):
  - The command list includes every `[NuruRoute]` endpoint (e.g. `card`, `version`, `help`).
  - Aliases are reported (`dotnetconf-stats`).
  - Rendered output written to a `TestTerminal` contains the commands, descriptions, and
    the REPL built-ins.
  - The list is sorted alphabetically.

## Phase 3: Independent review

- [x] A separate review agent (fresh context) reviews the full diff against both issues:
      correctness, missed requirements, test gaps, security, performance, accessibility.
- [x] Address significant findings, or record why one is skipped.

## Phase 4: Verification

- [x] `dotnet build && dotnet test && dotnet format --verify-no-changes` runs clean.
- [x] Manual check: `ardalis help` prints the command list; `ardalis -i` no longer crashes.
- [ ] PR CI is green, with no Node 20 deprecation warning and the coverage comment posted
      (checked after the PR opens).
