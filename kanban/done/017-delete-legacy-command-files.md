# Delete Legacy Command Files

## Summary

Delete all Spectre.Console command files from the Commands/ directory.

## Todo List

- [x] Delete `Commands/BlogCommand.cs`
- [x] Delete `Commands/BlueSkyCommand.cs`
- [x] Delete `Commands/BooksCommand.cs`
- [x] Delete `Commands/CardCommand.cs`
- [x] Delete `Commands/ContactCommand.cs`
- [x] Delete `Commands/CoursesCommand.cs`
- [x] Delete `Commands/DometrainCommand.cs`
- [x] Delete `Commands/DotNetConfScoreCommand.cs`
- [x] Delete `Commands/LinkedInCommand.cs`
- [x] Delete `Commands/NimbleProCommand.cs`
- [x] Delete `Commands/PackagesCommand.cs`
- [x] Delete `Commands/PluralsightCommand.cs`
- [x] Delete `Commands/QuoteCommand.cs`
- [x] Delete `Commands/RecentCommand.cs`
- [x] Delete `Commands/ReposCommand.cs`
- [x] Delete `Commands/SpeakerCommand.cs`
- [x] Delete `Commands/SubscribeCommand.cs`
- [x] Delete `Commands/TipCommand.cs`
- [x] Delete `Commands/YouTubeCommand.cs`
- [x] Delete `Commands/` directory
- [x] Delete `InteractiveMode.cs` (also legacy)
- [x] Delete `Helpers/PagingHelper.cs` (also legacy)
- [x] Verify `dotnet build` succeeds

## Notes

This is Phase 7b of the TimeWarp.Nuru migration.

All 19 command files are replaced by:
- URL constants in `Urls.cs`
- Handler files in `Handlers/`
- Route definitions in `Program.cs`

Files to delete:
- `Commands/*.cs` (19 files)
- `Commands/` directory

Reference: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md` - Phase 7

## Results

- Deleted all 19 command files from `Commands/` directory
- Deleted `Commands/` directory
- Deleted `InteractiveMode.cs` (legacy Spectre REPL mode)
- Deleted `Helpers/PagingHelper.cs` (legacy Spectre paging helper)
- Build succeeds with 0 warnings, 0 errors
- Total: 21 files deleted
