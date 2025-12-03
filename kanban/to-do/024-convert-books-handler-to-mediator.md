# Convert BooksHandler to Mediator Command

## Summary

Convert `BooksHandler.cs` to `BooksCommand.cs` using Mediator pattern with `IHttpClientFactory` injection.

## Todo List

- [ ] Create `Handlers/BooksCommand.cs` implementing `IRequest`
- [ ] Add properties: `bool NoPaging`, `int? Size`
- [ ] Add nested `Handler` class implementing `IRequestHandler<BooksCommand>`
- [ ] Inject `IHttpClientFactory` and `ILogger<Handler>`
- [ ] Use `_httpClientFactory.CreateClient("ArdalisWeb")` instead of static HttpClient
- [ ] Update `Program.cs` route: `.Map<BooksCommand>("books --no-paging? --page-size? {size:int?}", ...)`
- [ ] Delete `Handlers/BooksHandler.cs`
- [ ] Verify `ardalis books` works

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

Named client: `ArdalisWeb`
Parameters: `bool noPaging`, `int? size`
