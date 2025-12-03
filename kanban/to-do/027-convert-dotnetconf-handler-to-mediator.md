# Convert DotNetConfScoreHandler to Mediator Command

## Summary

Convert `DotNetConfScoreHandler.cs` to `DotNetConfScoreCommand.cs` using Mediator pattern with `IHttpClientFactory` injection.

## Todo List

- [ ] Create `Handlers/DotNetConfScoreCommand.cs` implementing `IRequest`
- [ ] Add property: `int? Year`
- [ ] Add nested `Handler` class implementing `IRequestHandler<DotNetConfScoreCommand>`
- [ ] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [ ] Use `_httpClientFactory.CreateClient("ArdalisApi")` for API calls
- [ ] Use `_httpClientFactory.CreateClient("ArdalisWeb")` for playlists.json
- [ ] Update `Program.cs` route: `.Map<DotNetConfScoreCommand>("dotnetconf-score {year:int?}", ...)`
- [ ] Delete `Handlers/DotNetConfScoreHandler.cs`
- [ ] Verify `ardalis dotnetconf-score` and `ardalis dotnetconf-score 2024` work

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named clients: `ArdalisApi`, `ArdalisWeb`
Parameters: `int? year`

This handler uses both the Ardalis API (for video stats) and ardalis.com (for playlists.json).
