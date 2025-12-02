# Remove Spectre.Console Packages

## Summary

Remove all Spectre.Console package references from the project.

## Todo List

- [x] Remove `Spectre.Console` package reference
- [x] Remove `Spectre.Console.Cli` package reference
- [x] Remove `Spectre.Console.Extensions.Logging` package reference
- [x] Verify no remaining Spectre.Console using statements in code
- [x] Verify `dotnet build` succeeds

## Notes

This is Phase 7a of the TimeWarp.Nuru migration.

`Spectre.Console.ImageSharp` should already be removed in task 002.

**DEPENDENCY**: This task requires tasks 017 and 018 to be completed first (delete legacy command files and infrastructure).

File to modify:
- `ardalis.csproj`

Reference: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md` - Phase 7

## Results

- Removed `Spectre.Console` package reference (0.53.0)
- Removed `Spectre.Console.Cli` package reference (0.53.0)
- Removed `Spectre.Console.Extensions.Logging` package reference (0.2.1)
- Verified no remaining Spectre.Console using statements in code
- Build succeeds with 0 warnings, 0 errors
