using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests;

/// <summary>
/// Tests for BooksEndpoint.Handler.Handle() — the #3 risk hotspot (CRAP 272,
/// cyclomatic complexity 16). The handler fetches books over HTTP and falls back
/// to hardcoded data when the request fails. Using a throwing factory exercises the
/// fallback path, the publication-date sort, and the display logic.
/// The single fallback book is fewer than the default page size of 10, so paging
/// (and Console.ReadKey) is never triggered without needing to force NoPaging.
/// </summary>
public class BooksEndpointHandlerTests
{
    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_DefaultOptions()
    {
        var handler = new BooksEndpoint.Handler(new ThrowingHttpClientFactory());
        // Fallback has 1 book; default pageSize (10) > 1 — ReadKey is never reached
        var query = new BooksEndpoint();

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_NoPagingFlag()
    {
        var handler = new BooksEndpoint.Handler(new ThrowingHttpClientFactory());
        var query = new BooksEndpoint { NoPaging = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_WithExplicitPageSize()
    {
        var handler = new BooksEndpoint.Handler(new ThrowingHttpClientFactory());
        var query = new BooksEndpoint { PageSize = 100 };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenNonHttpExceptionThrown()
    {
        var factory = new ThrowingHttpClientFactory(new InvalidOperationException("client setup error"));
        var handler = new BooksEndpoint.Handler(factory);
        var query = new BooksEndpoint { NoPaging = true };

        await handler.Handle(query, CancellationToken.None);
    }
}
