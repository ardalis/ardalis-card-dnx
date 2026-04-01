using System;
using System.Threading.Tasks;
using Ardalis.Cli.Telemetry;
using TimeWarp.Nuru;

namespace Ardalis.Cli.Behaviors;

/// <summary>
/// Nuru behavior that tracks command execution with PostHog.
/// This unified behavior applies to ALL routes (both endpoints and any remaining Fluent routes).
/// </summary>
public sealed class PostHogNuruBehavior(PostHogService postHog) : INuruBehavior
{
    public async ValueTask HandleAsync(BehaviorContext context, Func<ValueTask> proceed)
    {
        // Extract command name from the context
        string commandName = context.CommandName.ToLowerInvariant();
        postHog.TrackCommand(commandName);

        await proceed();
    }
}
