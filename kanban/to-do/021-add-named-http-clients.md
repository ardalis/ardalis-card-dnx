# Add Named HTTP Clients to ConfigureServices

## Summary

Add `IHttpClientFactory` with named HTTP clients to `Program.cs` ConfigureServices. This centralizes HTTP client configuration and prepares for converting handlers to Mediator commands.

## Todo List

- [ ] Add `Microsoft.Extensions.Http` package reference if needed
- [ ] Add named HTTP client "GitHub" (api.github.com, User-Agent, 10s timeout)
- [ ] Add named HTTP client "NuGet" (api-v2v3search-0.nuget.org, 10s timeout)
- [ ] Add named HTTP client "ArdalisApi" (api.ardalis.com, User-Agent, 30s timeout)
- [ ] Add named HTTP client "ArdalisWeb" (User-Agent, 10s timeout)
- [ ] Add named HTTP client "RssFeed" (10s timeout)
- [ ] Verify build succeeds

## Notes

Reference: `.agent/workspace/2025-12-03T18-00-00_di-migration-analysis.md`

```csharp
services.AddHttpClient("GitHub", client =>
{
    client.BaseAddress = new Uri("https://api.github.com/");
    client.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    client.Timeout = TimeSpan.FromSeconds(10);
});
```
