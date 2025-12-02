# Create Card Handler

## Summary

Create CardHandler using Nuru terminal widgets to display Ardalis business card.

## Todo List

- [x] Create `Handlers/` directory
- [x] Create `Handlers/CardHandler.cs`
- [x] Use `NuruTerminal.Default` for output
- [x] Use URL constants from `Urls.cs` with `using static`
- [x] Use `UrlHelper.AddUtmSource()` for tracking URLs
- [x] Implement panel with header, content, border styling
- [x] Use Nuru string extensions (`.Cyan()`, `.Bold()`, `.Link()`, etc.)
- [x] Verify `dotnet build` succeeds

## Notes

This is Phase 4b of the TimeWarp.Nuru migration.

CardHandler is a good first display handler because:
- No external dependencies (no HttpClient)
- No async work
- Tests panel widget, colors, links

No PostHogService injection needed - tracking handled by pipeline behavior.

File to create:
- `Handlers/CardHandler.cs`

Reference: `.agent/workspace/2025-12-02T16-30-00_timewarp-nuru-migration-plan.md` - Phase 4

## Results

- Created `Handlers/` directory
- Created `Handlers/CardHandler.cs` with Nuru terminal widgets
- Uses `using static` for URL constants and UrlHelper
- Panel with cyan/magenta styling, rounded border
- Top/bottom rules with color
- OSC 8 hyperlinks for all URLs
- Build succeeds with 0 warnings, 0 errors
