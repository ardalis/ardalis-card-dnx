# Create Packages Handler

## Summary

Create PackagesHandler using Nuru table widget to display NuGet packages with paging support.

## Todo List

- [x] Create `Handlers/PackagesHandler.cs`
- [x] Use `NuruTerminal.Default` for output
- [x] Port `GetPackagesFromApi()` logic from existing PackagesCommand
- [x] Port fallback packages list
- [x] Accept `bool all` and `int pageSize` parameters
- [x] Display packages in table with columns: Package, Downloads, Description
- [x] Implement paging logic (Space to continue)
- [x] Use URL constant for NuGet profile link
- [x] Verify `dotnet build` succeeds

## Notes

This is Phase 4f of the TimeWarp.Nuru migration.

PackagesHandler has optional parameters `--all?` and `--page-size {size:int?}`. These map to handler parameters.

File to create:
- `Handlers/PackagesHandler.cs`

Reference: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md` - Phase 4

## Results

- Created `Handlers/PackagesHandler.cs` (176 lines)
- Uses `NuruTerminal.Default` for all output
- Async `ExecuteAsync(bool showAll, int pageSize)` method
- Fetches packages from NuGet API with fallback to hardcoded list
- Table widget with Package (clickable link), Downloads, Description columns
- Paging with Space key to continue
- UTM tracking on package links via `UrlHelper.AddUtmSource()`
- Uses `Urls.NuGet` constant for profile link
- Build succeeds with 0 warnings, 0 errors
