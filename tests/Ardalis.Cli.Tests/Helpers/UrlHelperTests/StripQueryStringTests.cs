using Ardalis.Helpers;

namespace Ardalis.Cli.Tests.Helpers.UrlHelperTests;

/// <summary>
/// Tests for UrlHelper.StripQueryString — a pure string utility used throughout the CLI
/// to display clean URLs to users while opening tracked URLs in the browser.
/// </summary>
public class StripQueryStringTests
{
    [Test]
    public async Task ReturnsOriginalUrl_WhenNoQueryString()
    {
        var result = UrlHelper.StripQueryString("https://ardalis.com/blog");

        await Assert.That(result).IsEqualTo("https://ardalis.com/blog");
    }

    [Test]
    public async Task ReturnsUrlBeforeQuestionMark_WhenQueryStringPresent()
    {
        var result = UrlHelper.StripQueryString("https://ardalis.com/blog?foo=bar&baz=qux");

        await Assert.That(result).IsEqualTo("https://ardalis.com/blog");
    }

    [Test]
    public async Task ReturnsUrlBeforeQuestionMark_WhenOnlyUtmSourcePresent()
    {
        var result = UrlHelper.StripQueryString("https://ardalis.com/path?utm_source=ardaliscli");

        await Assert.That(result).IsEqualTo("https://ardalis.com/path");
    }

    [Test]
    public async Task ReturnsEmptyString_WhenInputIsEmpty()
    {
        var result = UrlHelper.StripQueryString(string.Empty);

        await Assert.That(result).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task ReturnsEverythingBeforeQuestionMark_WhenQueryIsEmpty()
    {
        var result = UrlHelper.StripQueryString("https://ardalis.com?");

        await Assert.That(result).IsEqualTo("https://ardalis.com");
    }

    [Test]
    public async Task PreservesDeepPath_WhenQueryStringPresent()
    {
        var result = UrlHelper.StripQueryString("https://ardalis.com/courses/clean-architecture?ref=homepage");

        await Assert.That(result).IsEqualTo("https://ardalis.com/courses/clean-architecture");
    }
}
