using System.Reflection;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.DotNetConfScoreEndpointTests;

/// <summary>
/// Tests for DotNetConfScoreEndpoint.Handler.ExtractYear — a private static regex
/// helper that extracts a 20xx year from a playlist name string.
/// Tested via reflection because the extraction logic is independently verifiable.
/// </summary>
public class ExtractYearTests
{
    private static readonly MethodInfo ExtractYear =
        typeof(DotNetConfScoreEndpoint)
            .GetNestedType("Handler", BindingFlags.Public | BindingFlags.NonPublic)!
            .GetMethod("ExtractYear", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("DotNetConfScoreEndpoint.Handler", "ExtractYear");

    private static int? Invoke(string name) =>
        (int?)ExtractYear.Invoke(null, [name]);

    [Test]
    public async Task ExtractYear_ReturnsYear_WhenNameContainsStandalone20xxYear()
    {
        var result = Invoke(".NET Conf 2024 Highlights");

        await Assert.That(result).IsEqualTo(2024);
    }

    [Test]
    public async Task ExtractYear_ReturnsFirstYear_WhenNameContainsMultipleYears()
    {
        var result = Invoke(".NET Conf 2024 and 2023 Sessions");

        await Assert.That(result).IsEqualTo(2024);
    }

    [Test]
    public async Task ExtractYear_ReturnsYear_WhenYearIsAtEndOfName()
    {
        var result = Invoke("Annual Conference 2025");

        await Assert.That(result).IsEqualTo(2025);
    }

    [Test]
    public async Task ExtractYear_ReturnsNull_WhenNameContainsNoYear()
    {
        var result = Invoke("General Sessions Playlist");

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task ExtractYear_ReturnsNull_WhenYearIsNotIn20xxRange()
    {
        var result = Invoke("Conference 1999");

        await Assert.That(result).IsNull();
    }
}
