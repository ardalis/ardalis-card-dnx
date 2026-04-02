using System.Net.Http;
using System.Reflection;
using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests;

/// <summary>
/// Tests for DotNetConfScoreEndpoint.Handler.Handle() — the #2 risk hotspot (CRAP 420,
/// cyclomatic complexity 20). The handler fetches playlist data over HTTP; when that
/// fails it catches HttpRequestException (with a special 403 sub-path) or generic
/// Exception. Using a throwing factory exercises both error branches.
///
/// Also tests the two private static regex helpers inside Handler:
///   ExtractVideoId — parses the v= param from a YouTube URL
///   ExtractYear    — extracts a 20xx year from a playlist name
/// </summary>
public class DotNetConfScoreEndpointHandlerTests
{
    // ── Handle() tests ──────────────────────────────────────────────────────────

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

    // ── ExtractVideoId reflection tests ─────────────────────────────────────────

    private static readonly MethodInfo ExtractVideoId =
        typeof(DotNetConfScoreEndpoint)
            .GetNestedType("Handler", BindingFlags.Public | BindingFlags.NonPublic)!
            .GetMethod("ExtractVideoId", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DotNetConfScoreEndpoint.Handler", "ExtractVideoId");

    private static string? InvokeExtractVideoId(string url) =>
        (string?)ExtractVideoId.Invoke(null, [url]);

    [Test]
    public async Task ExtractVideoId_ReturnsId_WhenVIsFirstQueryParam()
    {
        var result = InvokeExtractVideoId("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

        await Assert.That(result).IsEqualTo("dQw4w9WgXcQ");
    }

    [Test]
    public async Task ExtractVideoId_ReturnsId_WhenVFollowsOtherParam()
    {
        var result = InvokeExtractVideoId("https://www.youtube.com/watch?list=PLtest123&v=abc456");

        await Assert.That(result).IsEqualTo("abc456");
    }

    [Test]
    public async Task ExtractVideoId_ReturnsId_StoppingAtNextAmpersand()
    {
        var result = InvokeExtractVideoId("https://www.youtube.com/watch?v=abc456&t=30s");

        await Assert.That(result).IsEqualTo("abc456");
    }

    [Test]
    public async Task ExtractVideoId_ReturnsNull_WhenNoVParam()
    {
        var result = InvokeExtractVideoId("https://www.youtube.com/playlist?list=PLtest123");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ExtractVideoId_ReturnsNull_WhenUrlIsEmpty()
    {
        var result = InvokeExtractVideoId(string.Empty);

        await Assert.That(result).IsNull();
    }

    // ── ExtractYear reflection tests ─────────────────────────────────────────────

    private static readonly MethodInfo ExtractYear =
        typeof(DotNetConfScoreEndpoint)
            .GetNestedType("Handler", BindingFlags.Public | BindingFlags.NonPublic)!
            .GetMethod("ExtractYear", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DotNetConfScoreEndpoint.Handler", "ExtractYear");

    private static int? InvokeExtractYear(string name) =>
        (int?)ExtractYear.Invoke(null, [name]);

    [Test]
    public async Task ExtractYear_ReturnsYear_WhenNameContainsStandalone20xxYear()
    {
        var result = InvokeExtractYear(".NET Conf 2024 Highlights");

        await Assert.That(result).IsEqualTo(2024);
    }

    [Test]
    public async Task ExtractYear_ReturnsFirstYear_WhenNameContainsMultipleYears()
    {
        var result = InvokeExtractYear(".NET Conf 2024 and 2023 Sessions");

        await Assert.That(result).IsEqualTo(2024);
    }

    [Test]
    public async Task ExtractYear_ReturnsYear_WhenYearIsAtEndOfName()
    {
        var result = InvokeExtractYear("Annual Conference 2025");

        await Assert.That(result).IsEqualTo(2025);
    }

    [Test]
    public async Task ExtractYear_ReturnsNull_WhenNameContainsNoYear()
    {
        var result = InvokeExtractYear("General Sessions Playlist");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ExtractYear_ReturnsNull_WhenYearIsNotIn20xxRange()
    {
        var result = InvokeExtractYear("Conference 1999");

        await Assert.That(result).IsNull();
    }
}
