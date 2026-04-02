using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using TimeWarp.Terminal;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays version information and checks for updates.
/// </summary>
[NuruRoute("version", Description = "Display version and check for updates")]
public sealed class VersionEndpoint : IQuery<Unit>
{
    public sealed class Handler : IQueryHandler<VersionEndpoint, Unit>
    {
        public async ValueTask<Unit> Handle(VersionEndpoint query, CancellationToken ct)
        {
            ITerminal terminal = TimeWarpTerminal.Default;
            string version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0";

            terminal.WriteLine(version);

            // Check for updates on NuGet
            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
                string response = await httpClient.GetStringAsync("https://api.nuget.org/v3-flatcontainer/ardalis/index.json", ct);
                var versionData = System.Text.Json.JsonSerializer.Deserialize<NuGetVersionData>(response);

                if (versionData?.Versions != null && versionData.Versions.Length > 0)
                {
                    string latestVersion = versionData.Versions[^1];

                    var current = Version.Parse(version);
                    var latest = Version.Parse(latestVersion);

                    terminal.WriteLine();
                    if (latest > current)
                    {
                        terminal.WriteLine($"v{latestVersion} is available; upgrade with:".Yellow());
                        terminal.WriteLine("dotnet tool update -g ardalis".Cyan());
                    }
                    else
                    {
                        terminal.WriteLine("You are on the latest version.".Green());
                    }
                }
            }
            catch (Exception ex)
            {
                terminal.WriteLine($"Unable to check for updates: {ex.Message}".Red());
            }

            return default;
        }
    }
}
