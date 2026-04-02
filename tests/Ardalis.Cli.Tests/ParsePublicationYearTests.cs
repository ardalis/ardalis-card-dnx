using System.Reflection;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests;

/// <summary>
/// Tests for BooksEndpoint.Handler.ParsePublicationYear — a private static method that
/// normalises publication date strings of varying formats into an integer year for sorting.
/// It has five distinct code paths: empty input, plain year, DateTime-parseable string,
/// year embedded in a longer string, and an unrecognisable format.
/// Tested via reflection because the parsing logic is independently verifiable.
/// </summary>
public class ParsePublicationYearTests
{
    private static readonly MethodInfo ParsePublicationYear =
        typeof(BooksEndpoint)
            .GetNestedType("Handler", BindingFlags.Public | BindingFlags.NonPublic)!
            .GetMethod("ParsePublicationYear", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException("BooksEndpoint.Handler", "ParsePublicationYear");

    private static int Invoke(string input) =>
        (int)ParsePublicationYear.Invoke(null, [input])!;

    [Test]
    public async Task ReturnsZero_WhenInputIsEmpty()
    {
        var result = Invoke(string.Empty);

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task ReturnsZero_WhenInputIsWhitespace()
    {
        var result = Invoke("   ");

        await Assert.That(result).IsEqualTo(0);
    }

    [Test]
    public async Task ReturnsYear_WhenInputIsPlainFourDigitYear()
    {
        var result = Invoke("2023");

        await Assert.That(result).IsEqualTo(2023);
    }

    [Test]
    public async Task ReturnsYear_WhenInputIsFullDateTimeString()
    {
        var result = Invoke("January 15, 2021");

        await Assert.That(result).IsEqualTo(2021);
    }

    [Test]
    public async Task ReturnsYear_WhenInputIsIsoDateString()
    {
        var result = Invoke("2019-06-01");

        await Assert.That(result).IsEqualTo(2019);
    }

    [Test]
    public async Task ReturnsYear_WhenYearIsEmbeddedInLongerString()
    {
        var result = Invoke("Published in 2018 by Microsoft");

        await Assert.That(result).IsEqualTo(2018);
    }

    [Test]
    public async Task ReturnsYear_WhenInputContainsMonthAndYear()
    {
        var result = Invoke("March 2020");

        await Assert.That(result).IsEqualTo(2020);
    }

    [Test]
    public async Task ReturnsZero_WhenInputContainsNoRecognisableYear()
    {
        var result = Invoke("unknown");

        await Assert.That(result).IsEqualTo(0);
    }
}
