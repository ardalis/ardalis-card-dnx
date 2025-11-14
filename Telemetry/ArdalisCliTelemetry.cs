using System.Diagnostics;

namespace Ardalis.Cli.Telemetry;

/// <summary>
/// Provides a single ActivitySource for all Ardalis CLI components.
/// </summary>
internal sealed class ArdalisCliTelemetry
{
    /// <summary>
    /// The name of the ActivitySource for all CLI components.
    /// </summary>
    public const string ActivitySourceName = "Ardalis.Cli";

    /// <summary>
    /// The ActivitySource instance for all CLI components.
    /// </summary>
    public ActivitySource ActivitySource { get; } = new(ActivitySourceName);
}