using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Cli.Endpoints;

namespace Ardalis.Cli.Tests.Endpoints.DotNetConfScoreEndpointTests;

/// <summary>
/// Tests for DotNetConfScoreEndpoint.Handler.Handle() — the #2 risk hotspot (CRAP 420,
/// cyclomatic complexity 20). The handler fetches playlist data over HTTP; when that
/// fails it catches HttpRequestException (with a special 403 sub-path) or generic
/// Exception. Using a throwing factory exercises both error branches.
/// </summary>
public class HandlerTests
{
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

  [Test]
  public async Task Handle_UsesBuiltIn2026Playlist_WhenRemotePlaylistsJsonIsInvalid()
  {
    var factory = new RecordingHttpClientFactory(webResponse: "not json", webStatus: HttpStatusCode.OK);
    var handler = new DotNetConfScoreEndpoint.Handler(factory);
    var query = new DotNetConfScoreEndpoint { Year = 2026 };

    await handler.Handle(query, CancellationToken.None);

    await Assert.That(factory.ApiRequestPaths).Contains("/youtube/playlists/PLJL9Ln_f6sGM/stats");
  }

  [Test]
  public async Task Handle_UsesBuiltIn2026Playlist_WhenRemotePlaylistsRequestFails()
  {
    var factory = new RecordingHttpClientFactory(webResponse: "", webStatus: HttpStatusCode.InternalServerError);
    var handler = new DotNetConfScoreEndpoint.Handler(factory);
    var query = new DotNetConfScoreEndpoint { Year = 2026 };

    await handler.Handle(query, CancellationToken.None);

    await Assert.That(factory.ApiRequestPaths).Contains("/youtube/playlists/PLJL9Ln_f6sGM/stats");
  }

  [Test]
  public async Task Handle_UsesBuiltIn2026Playlist_WhenRemotePlaylistsLack2026()
  {
    const string remoteJson = """
            [{ "name": ".NET Conf 2025", "url": "https://www.youtube.com/playlist?list=PLremote2025" },
             { "name": null, "url": "https://www.youtube.com/playlist?list=PLnoname" }]
            """;
    var factory = new RecordingHttpClientFactory(webResponse: remoteJson, webStatus: HttpStatusCode.OK);
    var handler = new DotNetConfScoreEndpoint.Handler(factory);
    var query = new DotNetConfScoreEndpoint { Year = 2026 };

    await handler.Handle(query, CancellationToken.None);

    await Assert.That(factory.ApiRequestPaths).Contains("/youtube/playlists/PLJL9Ln_f6sGM/stats");
  }

  /// <summary>
  /// Serves a fixed playlists.json response for the "ArdalisWeb" client and records the
  /// request paths sent through the "ArdalisApi" client (which always returns 500).
  /// </summary>
  private sealed class RecordingHttpClientFactory(string webResponse, HttpStatusCode webStatus) : IHttpClientFactory
  {
    public ConcurrentBag<string> ApiRequestPaths { get; } = [];

    public HttpClient CreateClient(string name) => name == "ArdalisApi"
        ? new HttpClient(new RecordingHandler(ApiRequestPaths)) { BaseAddress = new Uri("https://api.example.test/") }
        : new HttpClient(new FakeHttpMessageHandler(webResponse, webStatus));

    private sealed class RecordingHandler(ConcurrentBag<string> paths) : HttpMessageHandler
    {
      protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
      {
        paths.Add(request.RequestUri!.AbsolutePath);
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
      }
    }
  }
}
