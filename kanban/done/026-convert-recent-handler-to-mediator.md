# Convert RecentHandler to Mediator Command

## Summary

Convert `RecentHandler.cs` to `RecentCommand.cs` using Mediator pattern with `IHttpClientFactory` injection. Also consolidate `RecentHelper.cs` functionality into the command.

## Todo List

- [x] Create `Handlers/RecentCommand.cs` implementing `IRequest`
- [x] Add property: `bool Verbose`
- [x] Add nested `Handler` class implementing `IRequestHandler<RecentCommand>`
- [x] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [x] Move `RecentHelper` logic into the handler
- [x] Use `_httpClientFactory.CreateClient("RssFeed")` for RSS/Atom fetching
- [x] Update `Program.cs` route: `.Map<RecentCommand>("recent --verbose?", ...)`
- [x] Delete `Handlers/RecentHandler.cs`
- [x] Delete `Helpers/RecentHelper.cs`
- [x] Verify `ardalis recent` and `ardalis recent --verbose` work

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named client: `RssFeed`
Parameters: `bool verbose`

This handler currently has 5 HttpClient instances via RecentHelper - consolidate to use IHttpClientFactory.

## Results

- Created `RecentCommand.cs` with nested `Handler` class following established patterns
- Consolidated all RSS/Atom parsing logic from `RecentHelper.cs` into the handler
- Uses `IHttpClientFactory.CreateClient("RssFeed")` for all feed fetching (4 sources: Blog, YouTube, GitHub, Bluesky)
- LinkedIn source removed as it returned empty list (no public RSS feed available)
- Both `ardalis recent` and `ardalis recent --verbose` work correctly
- Verbose mode shows per-source fetch status with result counts
- Build succeeds with no warnings or errors
