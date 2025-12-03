# Convert CoursesHandler to Mediator Command

## Summary

Convert `CoursesHandler.cs` to `CoursesCommand.cs` using Mediator pattern with `IHttpClientFactory` injection.

## Todo List

- [ ] Create `Handlers/CoursesCommand.cs` implementing `IRequest`
- [ ] Add properties: `bool All`, `int? Size`
- [ ] Add nested `Handler` class implementing `IRequestHandler<CoursesCommand>`
- [ ] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [ ] Use `_httpClientFactory.CreateClient("ArdalisWeb")` instead of static HttpClient
- [ ] Update `Program.cs` route: `.Map<CoursesCommand>("courses --all? --page-size? {size:int?}", ...)`
- [ ] Delete `Handlers/CoursesHandler.cs`
- [ ] Verify `ardalis courses` works

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named client: `ArdalisWeb`
Parameters: `bool all`, `int? size`
