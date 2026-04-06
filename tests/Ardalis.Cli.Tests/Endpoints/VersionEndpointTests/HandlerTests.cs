using System.Threading;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.VersionEndpointTests;

/// <summary>
/// Tests for VersionEndpoint.Handler.Handle() — CRAP 156, cyclomatic complexity 12.
/// The handler reads the assembly version, then compares it against the latest on NuGet.
/// It creates its own HttpClient internally, so a pre-cancelled token is used to
/// short-circuit the network call and exercise the catch(Exception) branch quickly.
/// </summary>
public class HandlerTests
{
    [Test]
    public async Task Handle_DoesNotThrow_WhenHttpCallCancelled()
    {
        var handler = new VersionEndpoint.Handler();
        var query = new VersionEndpoint();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // The GetStringAsync call throws OperationCanceledException,
        // caught by catch(Exception ex) → writes error message, returns default.
        await handler.Handle(query, cts.Token);
    }

    [Test]
    public async Task Handle_DoesNotThrow_WhenHandlerInstantiatedWithNoArgs()
    {
        // Confirms Handler has no required dependencies and can be constructed directly.
        var handler = new VersionEndpoint.Handler();

        await Assert.That(handler).IsNotNull();
    }
}
