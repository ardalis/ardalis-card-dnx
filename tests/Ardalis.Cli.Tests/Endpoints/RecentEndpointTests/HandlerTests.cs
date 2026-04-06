using System.Net.Http;
using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.RecentEndpointTests;

/// <summary>
/// Tests for RecentEndpoint.Handler.GetRecentActivitiesWithVerboseAsync() — CRAP 210,
/// cyclomatic complexity 14. The verbose path iterates each source sequentially and
/// has distinct catch branches for HttpRequestException (404/403/500/other),
/// TaskCanceledException, and generic Exception. Also exercises the non-verbose path
/// and the DisplayTable/GetRelativeTimeString code via a fake RSS feed.
/// </summary>
public class HandlerTests
{
    // Minimal RSS XML understood by ParseBlogRss and ParseBlueskyRss (both use
    // GetDefaultNamespace() and look for <item> descendants).
    private const string BlogRssXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <rss>
          <channel>
            <item>
              <title>Test Post</title>
              <link>https://ardalis.com/test-post</link>
              <pubDate>Mon, 16 Mar 2020 12:00:00 GMT</pubDate>
            </item>
          </channel>
        </rss>
        """;

    // --- Verbose=true: HttpRequestException branches ---

    // Note: RecentEndpoint.Handler calls CreateClient() *outside* its per-source try-catch,
    // so ThrowingHttpClientFactory (which throws in CreateClient) would propagate unhandled.
    // ThrowingOnRequestHttpClientFactory creates a real client whose SendAsync throws,
    // which is then caught by the per-source exception handlers.

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_Verbose()
    {
        var handler = new RecentEndpoint.Handler(new ThrowingOnRequestHttpClientFactory());
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttp404Error_Verbose()
    {
        var factory = new ThrowingOnRequestHttpClientFactory(new HttpRequestException("404 Not Found"));
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttp403Error_Verbose()
    {
        var factory = new ThrowingOnRequestHttpClientFactory(new HttpRequestException("403 Forbidden"));
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttp500Error_Verbose()
    {
        var factory = new ThrowingOnRequestHttpClientFactory(new HttpRequestException("500 Internal Server Error"));
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenTaskCanceled_Verbose()
    {
        var factory = new ThrowingOnRequestHttpClientFactory(new TaskCanceledException("Request timed out"));
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenGenericException_Verbose()
    {
        var factory = new ThrowingOnRequestHttpClientFactory(new InvalidOperationException("unexpected error"));
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }

    // --- Verbose=false ---

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_NotVerbose()
    {
        var handler = new RecentEndpoint.Handler(new ThrowingOnRequestHttpClientFactory());
        var query = new RecentEndpoint { Verbose = false };

        await handler.Handle(query, CancellationToken.None);
    }

    // --- Success path: exercises DisplayTable → GetRelativeTimeString ---

    [Test]
    public async Task Handle_DoesNotThrow_WhenFeedReturnsValidRss_NotVerbose()
    {
        // Blog + Bluesky parsers both find <item> in the RSS → non-empty activities list →
        // DisplayTable() is called → GetRelativeTimeString() is exercised.
        var factory = new FakeHttpClientFactory(BlogRssXml);
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = false };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenFeedReturnsValidRss_Verbose()
    {
        var factory = new FakeHttpClientFactory(BlogRssXml);
        var handler = new RecentEndpoint.Handler(factory);
        var query = new RecentEndpoint { Verbose = true };

        await handler.Handle(query, CancellationToken.None);
    }
}
