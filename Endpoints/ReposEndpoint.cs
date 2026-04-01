#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using TimeWarp.Terminal;
using static Ardalis.Cli.Urls;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays popular Ardalis GitHub repositories using Nuru table widget.
/// </summary>
[NuruRoute("repos", Description = "Display popular Ardalis GitHub repositories")]
public sealed class ReposEndpoint : IQuery<Unit>
{
    public sealed class Handler(
        IHttpClientFactory httpClientFactory) : IQueryHandler<ReposEndpoint, Unit>
    {
        private static readonly string[] RepoNames =
        [
            "CleanArchitecture",
            "Specification",
            "GuardClauses",
            "Result",
            "SmartEnum"
        ];

        public async ValueTask<Unit> Handle(ReposEndpoint query, CancellationToken ct)
        {
            ITerminal terminal = TimeWarpTerminal.Default;

            terminal.WriteLine("Ardalis's Popular GitHub Repositories".Green().Bold());
            terminal.WriteLine();

            HttpClient client = httpClientFactory.CreateClient("GitHub");
            List<(string Name, GitHubRepo Info)> repos = [];

            foreach (string repoName in RepoNames)
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(
                        $"repos/ardalis/{repoName}", ct);
                    response.EnsureSuccessStatusCode();

                    GitHubRepo? repoInfo = await response.Content
                        .ReadFromJsonAsync<GitHubRepo>(ct);

                    if (repoInfo != null)
                        repos.Add((repoName, repoInfo));
                }
                catch
                {
                    // Silently skip failed repo fetch
                }
            }

            repos.Sort((a, b) =>
                b.Info.StargazersCount.CompareTo(a.Info.StargazersCount));

            DisplayTable(repos);

            return default;
        }

        private static void DisplayTable(List<(string Name, GitHubRepo Info)> repos)
        {
            var terminal = TimeWarpTerminal.Default;

            terminal.WriteTable(table =>
            {
                table
                    .AddColumn("Repository")
                    .AddColumn("Stars", Alignment.Right)
                    .AddColumn("Description")
                    .Border(BorderStyle.Rounded);

                foreach ((string repoName, GitHubRepo repoInfo) in repos)
                {
                    string description = repoInfo.Description ?? "No description";
                    if (description.Length > 60)
                        description = description[..57] + "...";

                    table.AddRow(
                        repoName.Link(repoInfo.HtmlUrl).Cyan(),
                        $"\u2b50 {repoInfo.StargazersCount:N0}".Yellow(),
                        description.Gray()
                    );
                }
            });

            terminal.WriteLine();
            terminal.WriteLine("Visit: ".Gray() + GitHub.Link(GitHub).Cyan());
        }
    }

    private sealed class GitHubRepo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
    }
}
