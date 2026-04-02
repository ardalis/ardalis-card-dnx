using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Cli.Tests;

/// <summary>
/// A stub IHttpClientFactory that returns a fixed HTTP response for every request,
/// allowing handlers that fetch from HTTP to exercise their success-path code.
/// </summary>
internal sealed class FakeHttpMessageHandler(
    string responseContent,
    HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}

/// <summary>
/// A stub IHttpClientFactory backed by FakeHttpMessageHandler.
/// Supply an optional baseAddress when the handler under test uses relative URLs.
/// </summary>
internal sealed class FakeHttpClientFactory(
    string responseContent,
    HttpStatusCode statusCode = HttpStatusCode.OK,
    Uri? baseAddress = null) : IHttpClientFactory
{
    public HttpClient CreateClient(string name)
    {
        var client = new HttpClient(new FakeHttpMessageHandler(responseContent, statusCode));
        if (baseAddress != null)
            client.BaseAddress = baseAddress;
        return client;
    }
}

/// <summary>
/// An HttpMessageHandler that throws a configurable exception during SendAsync.
/// Used by ThrowingOnRequestHttpClientFactory so that CreateClient() succeeds (allowing
/// code that calls CreateClient outside a try-catch to proceed) but the actual HTTP
/// request fails — exercising catch blocks that guard individual request calls.
/// </summary>
internal sealed class ThrowingHttpMessageHandler(Exception exception) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromException<HttpResponseMessage>(exception);
}

/// <summary>
/// A stub IHttpClientFactory that creates a real HttpClient whose underlying handler
/// throws on every request. Unlike ThrowingHttpClientFactory (which throws in
/// CreateClient itself), this factory allows CreateClient to succeed and defers the
/// failure to the HTTP call — needed for handlers that call CreateClient outside their
/// try-catch blocks.
/// </summary>
internal sealed class ThrowingOnRequestHttpClientFactory(Exception? exception = null) : IHttpClientFactory
{
    private readonly Exception _exception = exception ?? new HttpRequestException("Network unavailable");

    public HttpClient CreateClient(string name)
        => new HttpClient(new ThrowingHttpMessageHandler(_exception));
}
