using System.Net.Http;
using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.DotNetConfScoreEndpointTests;

/// <summary>
/// Tests for DotNetConfScoreEndpoint.Handler.Handle() — the #2 risk hotspot (CRAP 420,
/// cyclomatic complexity 20). The handler fetches playlist data over HTTP; when that
/// fails it catches HttpRequestException (with a special 403 sub-path) or generic
/// Exception. Using a throwing factory exercises both error branches.
/// </summary>
public class HandlerTests
{
    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_WithProvidedYear()
    {
        var handler = new DotNetConfScoreEndpoint.Handler(new ThrowingHttpClientFactory());
        var query = new DotNetConfScoreEndpoint { Year = 2024 };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_WithDefaultYear()
    {
        var handler = new DotNetConfScoreEndpoint.Handler(new ThrowingHttpClientFactory());
        // Year = null triggers the GetDefaultYear() path
        var query = new DotNetConfScoreEndpoint();

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_With403Message()
    {
        var factory = new ThrowingHttpClientFactory(new HttpRequestException("403 Forbidden"));
        var handler = new DotNetConfScoreEndpoint.Handler(factory);
        var query = new DotNetConfScoreEndpoint { Year = 2023 };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenGenericExceptionThrown()
    {
        var factory = new ThrowingHttpClientFactory(new InvalidOperationException("unexpected"));
        var handler = new DotNetConfScoreEndpoint.Handler(factory);
        var query = new DotNetConfScoreEndpoint { Year = 2024 };

        await handler.Handle(query, CancellationToken.None);
    }
}
