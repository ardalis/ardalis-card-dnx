# Convert ReposHandler to Mediator Command

## Summary

Convert `ReposHandler.cs` to `ReposCommand.cs` using Mediator pattern with `IHttpClientFactory` injection.

## Todo List

- [ ] Create `Handlers/ReposCommand.cs` implementing `IRequest`
- [ ] Add nested `Handler` class implementing `IRequestHandler<ReposCommand>`
- [ ] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [ ] Use `_httpClientFactory.CreateClient("GitHub")` instead of static HttpClient
- [ ] Update `Program.cs` route: `.Map<ReposCommand>("repos", ...)`
- [ ] Delete `Handlers/ReposHandler.cs`
- [ ] Verify `ardalis repos` works

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named client: `GitHub`
Parameters: None
