# Convert RecentHandler to Mediator Command

## Summary

Convert `RecentHandler.cs` to `RecentCommand.cs` using Mediator pattern with `IHttpClientFactory` injection. Also consolidate `RecentHelper.cs` functionality into the command.

## Todo List

- [ ] Create `Handlers/RecentCommand.cs` implementing `IRequest`
- [ ] Add property: `bool Verbose`
- [ ] Add nested `Handler` class implementing `IRequestHandler<RecentCommand>`
- [ ] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [ ] Move `RecentHelper` logic into the handler
- [ ] Use `_httpClientFactory.CreateClient("RssFeed")` for RSS/Atom fetching
- [ ] Update `Program.cs` route: `.Map<RecentCommand>("recent --verbose?", ...)`
- [ ] Delete `Handlers/RecentHandler.cs`
- [ ] Delete `Helpers/RecentHelper.cs`
- [ ] Verify `ardalis recent` and `ardalis recent --verbose` work

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named client: `RssFeed`
Parameters: `bool verbose`

This handler currently has 5 HttpClient instances via RecentHelper - consolidate to use IHttpClientFactory.
