using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PostHog;

namespace Ardalis.Cli.Telemetry;

/// <summary>
/// HTTP client factory that logs all requests for debugging PostHog issues.
/// </summary>
public class LoggingHttpClientFactory : IHttpClientFactory
{
    private readonly ILogger<LoggingHttpClientFactory> _logger;

    public LoggingHttpClientFactory(ILogger<LoggingHttpClientFactory> logger)
    {
        _logger = logger;
    }

    public HttpClient CreateClient(string name)
    {
        var handler = new LoggingHttpMessageHandler(_logger);
        var client = new HttpClient(handler);

        _logger.LogDebug("Created HttpClient for: {Name}", name);

        return client;
    }
}

/// <summary>
/// HTTP message handler that logs all requests and responses.
/// </summary>
public class LoggingHttpMessageHandler : HttpClientHandler
{
    private readonly ILogger _logger;

    public LoggingHttpMessageHandler(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        System.Threading.CancellationToken cancellationToken)
    {
        _logger.LogInformation("HTTP {Method} {Uri}", request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsStringAsync();
            _logger.LogDebug("Request body: {Body}", content.Length > 500 ? content.Substring(0, 500) + "..." : content);
        }

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            _logger.LogInformation("HTTP {StatusCode} from {Uri}",
                (int)response.StatusCode, request.RequestUri);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("HTTP Error Response: {Body}", errorBody);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HTTP request failed to {Uri}", request.RequestUri);
            throw;
        }
    }
}
