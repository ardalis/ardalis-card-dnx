using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.PackagesEndpointTests;

/// <summary>
/// Tests for PackagesEndpoint.Handler.GetPackagesFromApiAsync() — CRAP 156,
/// cyclomatic complexity 12. The method fetches packages from the NuGet search API,
/// filters them by package-name depth when showAll=false, and returns null on any
/// failure (triggering the FallbackPackages path in Handle). Tests cover the
/// exception/fallback path via ThrowingHttpClientFactory, and the success paths
/// (all packages, filtered, empty data) via FakeHttpClientFactory with JSON.
/// All=true or large PageSize avoids Console.ReadKey in the paging loop.
/// </summary>
public class HandlerTests
{
    // Valid NuGet search response with one top-level and one nested package.
    // "Ardalis.Test" has 1 dot → included when showAll=false (count <= 1).
    // "Ardalis.Test.Sub.Package" has 3 dots → excluded when showAll=false.
    private const string NuGetJsonWithPackages = """
        {
          "data": [
            {"id": "Ardalis.Test", "description": "Top-level test package", "totalDownloads": 5000},
            {"id": "Ardalis.Test.Sub.Package", "description": "Deep nested package", "totalDownloads": 100}
          ]
        }
        """;

    private const string NuGetJsonNullData = """{"data": null}""";

    private static readonly Uri NuGetBase = new("https://azuresearch-usnc.nuget.org/query/");

    // --- Fallback path (HTTP failure) ---

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_AllFlag()
    {
        var handler = new PackagesEndpoint.Handler(new ThrowingHttpClientFactory());
        var query = new PackagesEndpoint { All = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_LargePageSize()
    {
        var handler = new PackagesEndpoint.Handler(new ThrowingHttpClientFactory());
        // Fallback has 7 packages; PageSize=100 means paging never fires
        var query = new PackagesEndpoint { PageSize = 100 };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpRequestFails_DefaultOptions()
    {
        var handler = new PackagesEndpoint.Handler(new ThrowingHttpClientFactory());
        // Default PageSize=10 > 7 fallback items → ReadKey never reached
        var query = new PackagesEndpoint();

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenNonHttpExceptionThrown()
    {
        var factory = new ThrowingHttpClientFactory(new InvalidOperationException("client setup error"));
        var handler = new PackagesEndpoint.Handler(factory);
        var query = new PackagesEndpoint { All = true };

        await handler.Handle(query, CancellationToken.None);
    }

    // --- Success path via FakeHttpClientFactory ---

    [Test]
    public async Task Handle_DoesNotThrow_WhenNuGetApiReturnsPackages_AllFlag()
    {
        // showAll=true: both packages (1-dot and 3-dot) are returned without filtering.
        var factory = new FakeHttpClientFactory(NuGetJsonWithPackages, baseAddress: NuGetBase);
        var handler = new PackagesEndpoint.Handler(factory);
        var query = new PackagesEndpoint { All = true };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenNuGetApiReturnsPackages_FilteredByDepth()
    {
        // showAll=false: only "Ardalis.Test" (1 dot) passes the <= 1 dot filter.
        // LargePageSize avoids ReadKey.
        var factory = new FakeHttpClientFactory(NuGetJsonWithPackages, baseAddress: NuGetBase);
        var handler = new PackagesEndpoint.Handler(factory);
        var query = new PackagesEndpoint { PageSize = 100 };

        await handler.Handle(query, CancellationToken.None);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenNuGetApiReturnsNullData_FallsBackToHardcoded()
    {
        // searchResult.Data == null → GetPackagesFromApiAsync returns null → FallbackPackages used.
        var factory = new FakeHttpClientFactory(NuGetJsonNullData, baseAddress: NuGetBase);
        var handler = new PackagesEndpoint.Handler(factory);
        var query = new PackagesEndpoint { All = true };

        await handler.Handle(query, CancellationToken.None);
    }
}
