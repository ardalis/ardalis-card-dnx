using System.Reflection;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.DotNetConfScoreEndpointTests;

/// <summary>
/// Tests for DotNetConfScoreEndpoint.Handler.ExtractVideoId — a private static regex
/// helper that parses the v= parameter from a YouTube URL.
/// Tested via reflection because the extraction logic is independently verifiable.
/// </summary>
public class ExtractVideoIdTests
{
    private static readonly MethodInfo ExtractVideoId =
        typeof(DotNetConfScoreEndpoint)
            .GetNestedType("Handler", BindingFlags.Public | BindingFlags.NonPublic)!
            .GetMethod("ExtractVideoId", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DotNetConfScoreEndpoint.Handler", "ExtractVideoId");

    private static string? Invoke(string url) =>
        (string?)ExtractVideoId.Invoke(null, [url]);

    [Test]
    public async Task ExtractVideoId_ReturnsId_WhenVIsFirstQueryParam()
    {
        var result = Invoke("https://www.youtube.com/watch?v=dQw4w9WgXcQ");

        await Assert.That(result).IsEqualTo("dQw4w9WgXcQ");
    }

    [Test]
    public async Task ExtractVideoId_ReturnsId_WhenVFollowsOtherParam()
    {
        var result = Invoke("https://www.youtube.com/watch?list=PLtest123&v=abc456");

        await Assert.That(result).IsEqualTo("abc456");
    }

    [Test]
    public async Task ExtractVideoId_ReturnsId_StoppingAtNextAmpersand()
    {
        var result = Invoke("https://www.youtube.com/watch?v=abc456&t=30s");

        await Assert.That(result).IsEqualTo("abc456");
    }

    [Test]
    public async Task ExtractVideoId_ReturnsNull_WhenNoVParam()
    {
        var result = Invoke("https://www.youtube.com/playlist?list=PLtest123");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ExtractVideoId_ReturnsNull_WhenUrlIsEmpty()
    {
        var result = Invoke(string.Empty);

        await Assert.That(result).IsNull();
    }
}
