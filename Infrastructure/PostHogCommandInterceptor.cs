using System;
using System.Collections.Generic;
using Ardalis.Cli.Telemetry;
using Spectre.Console.Cli;

namespace Ardalis.Cli.Infrastructure;

/// <summary>
/// Interceptor that tracks command execution with PostHog.
/// This is registered once and automatically tracks all commands.
/// </summary>
public class PostHogCommandInterceptor : ICommandInterceptor
{
    private readonly PostHogService _postHog;

    public PostHogCommandInterceptor(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public void Intercept(CommandContext context, CommandSettings settings)
    {
        // Get the command name from the context
        var commandName = context.Name ?? "(none)";

        // Track the command execution
        _postHog.TrackCommand(commandName, new Dictionary<string, object>
        {
            ["has_settings"] = settings != null,
            ["settings_type"] = settings?.GetType().Name ?? "none"
        });
    }

    public void InterceptResult(CommandContext context, CommandSettings settings, ref int result)
    {
        // Optional: Track command completion with exit code
        // Currently not tracking this to keep events simple
    }
}
