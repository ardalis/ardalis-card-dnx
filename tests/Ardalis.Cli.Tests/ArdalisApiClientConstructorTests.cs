using Ardalis.Api;

namespace Ardalis.Cli.Tests;

/// <summary>
/// Tests for ArdalisApiClient constructor guard clauses — the constructors enforce
/// API key presence at object creation time, preventing misconfigured clients from
/// making requests that would silently fail at runtime.
/// </summary>
public class ArdalisApiClientConstructorTests
{
    // --- (string apiKey) overload ---

    [Test]
    public async Task ThrowsArgumentException_WhenApiKeyIsNull()
    {
        await Assert.That(() => new ArdalisApiClient(null!)).Throws<ArgumentException>();
    }

    [Test]
    public async Task ThrowsArgumentException_WhenApiKeyIsEmpty()
    {
        await Assert.That(() => new ArdalisApiClient(string.Empty)).Throws<ArgumentException>();
    }

    [Test]
    public async Task ThrowsArgumentException_WhenApiKeyIsWhitespace()
    {
        await Assert.That(() => new ArdalisApiClient("   ")).Throws<ArgumentException>();
    }

    [Test]
    public async Task Succeeds_WhenApiKeyIsValid()
    {
        using var client = new ArdalisApiClient("valid-api-key");

        await Assert.That(client).IsNotNull();
    }

    // --- (HttpClient, string apiKey) overload ---

    [Test]
    public async Task ThrowsArgumentNullException_WhenHttpClientIsNull()
    {
        await Assert.That(() => new ArdalisApiClient((HttpClient)null!, "valid-api-key"))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task ThrowsArgumentException_WhenApiKeyIsNullWithHttpClientOverload()
    {
        using var httpClient = new HttpClient();

        await Assert.That(() => new ArdalisApiClient(httpClient, null!))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ThrowsArgumentException_WhenApiKeyIsEmptyWithHttpClientOverload()
    {
        using var httpClient = new HttpClient();

        await Assert.That(() => new ArdalisApiClient(httpClient, string.Empty))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Succeeds_WhenBothHttpClientAndApiKeyAreValid()
    {
        using var httpClient = new HttpClient();
        using var client = new ArdalisApiClient(httpClient, "valid-api-key");

        await Assert.That(client).IsNotNull();
    }
}
