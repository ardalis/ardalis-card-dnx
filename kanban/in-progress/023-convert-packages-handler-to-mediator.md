# Convert PackagesHandler to Mediator Command

## Summary

Convert `PackagesHandler.cs` to `PackagesCommand.cs` using Mediator pattern with `IHttpClientFactory` injection.

## Todo List

- [ ] Create `Handlers/PackagesCommand.cs` implementing `IRequest`
- [ ] Add properties: `bool All`, `int? Size`
- [ ] Add nested `Handler` class implementing `IRequestHandler<PackagesCommand>`
- [ ] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [ ] Use `_httpClientFactory.CreateClient("NuGet")` instead of static HttpClient
- [ ] Update `Program.cs` route: `.Map<PackagesCommand>("packages --all? --page-size? {size:int?}", ...)`
- [ ] Delete `Handlers/PackagesHandler.cs`
- [ ] Verify `ardalis packages` and `ardalis packages --all` work

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named client: `NuGet`
Parameters: `bool all`, `int? size`
