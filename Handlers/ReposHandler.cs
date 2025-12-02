#nullable enable
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using static Ardalis.Cli.Urls;

namespace Ardalis.Cli.Handlers;

/// <summary>
/// Displays popular Ardalis GitHub repositories using Nuru table widget.
/// </summary>
public static class ReposHandler
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly string[] RepoNames =
    [
        "CleanArchitecture",
        "Specification",
        "GuardClauses",
        "Result",
        "SmartEnum"
    ];

    static ReposHandler()
    {
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    }

    public static async Task ExecuteAsync()
    {
        ITerminal terminal = NuruTerminal.Default;

        terminal.WriteLine("Ardalis's Popular GitHub Repositories".Green().Bold());
        terminal.WriteLine();

        // Fetch all repo info
        List<(string Name, GitHubRepo Info)> repos = [];
        foreach (string repoName in RepoNames)
        {
            try
            {
                GitHubRepo? repoInfo = await GetRepoInfoAsync(repoName);
                if (repoInfo != null)
                {
                    repos.Add((repoName, repoInfo));
                }
            }
            catch
            {
                // Skip repos that fail to load
            }
        }

        // Sort by stars descending
        repos.Sort((a, b) => b.Info.StargazersCount.CompareTo(a.Info.StargazersCount));

        // Build table
        Table table = new Table()
            .AddColumn("Repository")
            .AddColumn("Stars", Alignment.Right)
            .AddColumn("Description");

        table.Border = BorderStyle.Rounded;

        foreach ((string repoName, GitHubRepo repoInfo) in repos)
        {
            string stars = $"⭐ {repoInfo.StargazersCount:N0}";
            string description = repoInfo.Description ?? "No description";

            // Truncate description if too long
            if (description.Length > 60)
            {
                description = description[..57] + "...";
            }

            table.AddRow(
                repoName.Link(repoInfo.HtmlUrl).Cyan(),
                stars.Yellow(),
                description.Gray()
            );
        }

        terminal.WriteTable(table);
        terminal.WriteLine();
        terminal.WriteLine("Visit: ".Gray() + GitHub.Link(GitHub).Cyan());
    }

    private static async Task<GitHubRepo?> GetRepoInfoAsync(string repoName)
    {
        string url = $"https://api.github.com/repos/ardalis/{repoName}";
        string response = await HttpClient.GetStringAsync(url);
        return JsonSerializer.Deserialize<GitHubRepo>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    private sealed class GitHubRepo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
    }
}
