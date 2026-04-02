using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PostHog;

namespace Ardalis.Cli.Telemetry;

/// <summary>
/// Service for tracking CLI usage events with PostHog.
/// </summary>
public sealed class PostHogService : IDisposable
{
    private readonly PostHogClient _client;
    private readonly ILogger<PostHogService> _logger;
    private readonly bool _isEnabled;

    public PostHogService(
        IConfiguration configuration,
        ILogger<PostHogService> logger,
        ILoggerFactory loggerFactory)
    {
        _logger = logger;

        var apiKey = configuration["POSTHOG_API_KEY"] ?? "phc_VUWlfJoBqITLdMOlJ4uAiYmvts8XWimPqzzXdU6ZnrU";
        // Add trailing slash to ensure /batch is appended correctly
        // webhook test site: https://webhook.site/0c394d0f-bcd8-4034-a91f-f0d64d341fc6/
        var host = configuration["POSTHOG_HOST"] ?? "https://us.i.posthog.com/";

        _logger.LogInformation("PostHog configuration - API Key: {ApiKey}, Host: {Host}",
            apiKey.Substring(0, Math.Min(10, apiKey.Length)) + "...", host);
        _logger.LogInformation("Events will be sent to: {Endpoint}", $"{host}/batch");

        if (!string.IsNullOrEmpty(apiKey))
        {
            var options = Options.Create(new PostHogOptions
            {
                ProjectApiKey = apiKey,
                HostUrl = new Uri(host),
                // For CLI tools, send events immediately instead of batching
                // This ensures events are sent before the process exits
                FlushAt = 1, // Send after every event instead of batching
                FlushInterval = TimeSpan.FromMilliseconds(100) // Send quickly
            });

            // Create HTTP client factory with logging
            var httpClientFactory = new LoggingHttpClientFactory(
                loggerFactory.CreateLogger<LoggingHttpClientFactory>());

            _client = new PostHogClient(
                options,
                httpClientFactory: httpClientFactory,
                loggerFactory: loggerFactory);
            _isEnabled = true;
            _logger.LogInformation("PostHog client initialized with host: {Host}", host);
        }
        else
        {
            // Create a dummy client for disabled state
            var options = Options.Create(new PostHogOptions());
            _client = new PostHogClient(options);
            _isEnabled = false;
            _logger.LogDebug("PostHog disabled - no API key configured");
        }
    }

    /// <summary>
    /// Track a command execution event.
    /// </summary>
    public void TrackCommand(string commandName, Dictionary<string, object> properties = null)
    {
        if (!_isEnabled) return;

        try
        {
            var distinctId = GetOrCreateDistinctId();
            var props = properties ?? new Dictionary<string, object>();

            // Add common properties
            props["command"] = commandName;
            props["cli_version"] = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
            props["os"] = Environment.OSVersion.Platform.ToString();

            _logger.LogInformation("Calling PostHog Capture with distinctId: {DistinctId}, event: command_executed", distinctId);
            _client.Capture(distinctId, "command_executed", props);
            _logger.LogInformation("Tracked command: {Command} - event queued for sending", commandName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track command: {Command}", commandName);
        }
    }

    /// <summary>
    /// Track interactive mode usage.
    /// </summary>
    public void TrackInteractiveMode(bool started)
    {
        if (!_isEnabled) return;

        try
        {
            var distinctId = GetOrCreateDistinctId();
            var eventName = started ? "interactive_mode_started" : "interactive_mode_exited";

            _client.Capture(distinctId, eventName, new Dictionary<string, object>
            {
                ["cli_version"] = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown"
            });

            _logger.LogDebug("Tracked interactive mode: {Event}", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to track interactive mode event");
        }
    }

    /// <summary>
    /// Gets or creates a persistent distinct ID for this user.
    /// </summary>
    private string GetOrCreateDistinctId()
    {
        // Use a combination of machine name and user name for anonymous tracking
        // This creates a stable ID per machine/user combination without PII
        var machineName = Environment.MachineName;
        var userName = Environment.UserName;

        // Create a hash to avoid sending actual machine/user names
        var combined = $"{machineName}_{userName}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hashBytes)[..16]; // Use first 16 chars of hash
    }

    public void Dispose()
    {
        try
        {
            // PostHog SDK batches events and sends them asynchronously
            // With FlushAt=1, events are typically sent immediately upon capture
            // This flush ensures any remaining queued events are sent
            _logger.LogDebug("Flushing any remaining PostHog events (events with FlushAt=1 are sent immediately on capture)");

            // Fire and forget - don't wait
            _ = _client?.FlushAsync();

            // Minimal delay to let the flush operation start
            System.Threading.Thread.Sleep(100);

            _logger.LogDebug("PostHog client disposal complete");
            _client?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing PostHog client");
        }
    }
}
