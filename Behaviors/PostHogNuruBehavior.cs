using System;
using System.Threading.Tasks;
using Ardalis.Cli.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using TimeWarp.Nuru;

namespace Ardalis.Cli.Behaviors;

/// <summary>
/// Nuru behavior that tracks command execution with PostHog.
/// This unified behavior applies to ALL routes (both endpoints and any remaining Fluent routes).
/// Self-contained: creates its own PostHogService to work around a Nuru beta generator bug
/// where behavior constructor DI parameters are not resolved via the MS DI service provider.
/// </summary>
public sealed class PostHogNuruBehavior : INuruBehavior
{
    private static readonly Lazy<PostHogService> _service = new(CreateService);

    private static PostHogService CreateService()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
        var loggerFactory = NullLoggerFactory.Instance;
        return new PostHogService(configuration, NullLogger<PostHogService>.Instance, loggerFactory);
    }

    public async ValueTask HandleAsync(BehaviorContext context, Func<ValueTask> proceed)
    {
        string commandName = context.CommandName.ToLowerInvariant();
        _service.Value.TrackCommand(commandName);
        await proceed();
    }
}
