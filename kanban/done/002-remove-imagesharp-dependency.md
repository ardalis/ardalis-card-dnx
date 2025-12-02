# Remove ImageSharp Dependency

## Summary

Remove `--with-covers` option from BooksCommand and remove Spectre.Console.ImageSharp package.

## Todo List

- [x] Remove `WithCovers` property from `BooksCommand.Settings`
- [x] Remove image rendering code from `DisplayBook` method
- [x] Update `DisplayBook` signature (remove `withCovers` parameter)
- [x] Update caller in `ExecuteAsync` 
- [x] Remove ImageSharp using statements
- [x] Remove `Spectre.Console.ImageSharp` package reference from csproj
- [x] Verify `dotnet build` succeeds
- [x] Verify `ardalis books` works without --with-covers
- [x] Verify `ardalis books --help` no longer shows --with-covers option

## Notes

This is Phase 2 of the TimeWarp.Nuru migration. The --with-covers feature is being removed entirely, not replaced.

Files to modify:
- `Commands/BooksCommand.cs`
- `ardalis.csproj`

Reference: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md` - Phase 2

## Results

- Removed `WithCovers` property from Settings class
- Removed `System.IO` using (no longer needed without MemoryStream)
- Changed `DisplayBook` from `async Task` to `void` (no longer async without image fetching)
- Simplified `PagingHelper.DisplayWithPaging` call to use method group `DisplayBook`
- Removed `Spectre.Console.ImageSharp` package reference
- Build succeeds with 0 warnings, 0 errors
- `ardalis books --help` no longer shows `--with-covers` option
