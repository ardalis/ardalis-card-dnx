using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.CoursesEndpointTests;

/// <summary>
/// Tests for CoursesEndpoint.Handler.Handle() — the #1 risk hotspot (CRAP 420,
/// cyclomatic complexity 20). The handler fetches courses over HTTP and falls back
/// to hardcoded data when the request fails. Using a throwing factory exercises the
/// fallback path and all subsequent display logic without network I/O.
/// All=true / large PageSize avoids Console.ReadKey blocking in tests.
/// </summary>
public class HandlerTests
{
    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_AllFlag()
    {
        var handler = new CoursesEndpoint.Handler(new ThrowingHttpClientFactory());
        var query = new CoursesEndpoint { All = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_LargePageSize()
    {
        var handler = new CoursesEndpoint.Handler(new ThrowingHttpClientFactory());
        // PageSize larger than fallback count (2) avoids Console.ReadKey
        var query = new CoursesEndpoint { PageSize = 100 };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_DefaultOptions()
    {
        var handler = new CoursesEndpoint.Handler(new ThrowingHttpClientFactory());
        // Fallback has 2 courses; default pageSize is 10 — paging never fires
        var query = new CoursesEndpoint();

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenNonHttpExceptionThrown()
    {
        var factory = new ThrowingHttpClientFactory(new InvalidOperationException("client setup error"));
        var handler = new CoursesEndpoint.Handler(factory);
        var query = new CoursesEndpoint { All = true };

        await handler.Handle(query, CancellationToken.None);
    }
}
