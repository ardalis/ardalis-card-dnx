using System.Net.Http;

namespace Ardalis.Cli.Tests;

/// <summary>
/// A stub IHttpClientFactory that throws on every CreateClient call, forcing handlers
/// to exercise their fallback / error-handling code paths.
/// </summary>
internal sealed class ThrowingHttpClientFactory : IHttpClientFactory
{
    private readonly Exception _exception;

    public ThrowingHttpClientFactory(Exception? exception = null)
    {
        _exception = exception ?? new HttpRequestException("Network unavailable");
    }

    public HttpClient CreateClient(string name) => throw _exception;
}
