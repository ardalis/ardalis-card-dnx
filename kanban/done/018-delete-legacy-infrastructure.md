# Delete Legacy Infrastructure

## Summary

Delete TypeRegistrar, InteractiveMode, and other legacy files no longer needed with Nuru.

## Todo List

- [x] Delete `Infrastructure/TypeRegistrar.cs`
- [x] Delete `Infrastructure/` directory (if empty)
- [x] Delete `InteractiveMode.cs` (done in task 017)
- [x] Delete `Helpers/PagingHelper.cs` (done in task 017)
- [ ] Keep `Program.Spectre.cs.bak` (for reference)
- [x] Verify `dotnet build` succeeds

## Notes

This is Phase 7c of the TimeWarp.Nuru migration.

These files are replaced by:
- `TypeRegistrar.cs` -> Nuru's built-in DI integration
- `InteractiveMode.cs` -> Nuru's built-in REPL mode
- `PagingHelper.cs` -> May be reimplemented in handlers or use Nuru equivalent

Files to delete:
- `Infrastructure/TypeRegistrar.cs`
- `InteractiveMode.cs`
- `Helpers/PagingHelper.cs`
- `Program.Spectre.cs.bak`

Reference: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md` - Phase 7

## Results

- Deleted `Infrastructure/TypeRegistrar.cs` (Spectre.Console.Cli DI registrar)
- Deleted `Infrastructure/` directory
- `InteractiveMode.cs` and `PagingHelper.cs` already deleted in task 017
- Kept `Program.Spectre.cs.bak` for reference
- Build succeeds with 0 warnings, 0 errors
