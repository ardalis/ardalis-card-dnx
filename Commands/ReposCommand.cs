using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class ReposCommand : AsyncCommand
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly string[] RepoNames = new[]
    {
        "CleanArchitecture",
        "ApiEndpoints",
        "Specification",
        "GuardClauses",
        "Result",
        "SmartEnum"
    };

    static ReposCommand()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    }

    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine("[bold green]Ardalis's Popular GitHub Repositories[/]\n");

        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[bold]Repository[/]");
        table.AddColumn("[bold]Stars[/]");
        table.AddColumn("[bold]Description[/]");

        foreach (var repoName in RepoNames)
        {
            try
            {
                var repoInfo = await GetRepoInfo(repoName);
                if (repoInfo != null)
                {
                    var stars = repoInfo.StargazersCount.ToString("N0");
                    var description = repoInfo.Description ?? "No description";
                    
                    // Truncate description if too long
                    if (description.Length > 60)
                    {
                        description = description.Substring(0, 57) + "...";
                    }

                    table.AddRow(
                        $"[link={repoInfo.HtmlUrl}]{repoName}[/]",
                        $"[yellow]⭐ {stars}[/]",
                        $"[dim]{description}[/]"
                    );
                }
            }
            catch
            {
                // Skip repos that fail to load
                table.AddRow(
                    $"{repoName}",
                    "[dim]N/A[/]",
                    "[dim]Failed to load[/]"
                );
            }
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Visit: [link]https://github.com/ardalis[/][/]");

        return 0;
    }

    private static async Task<GitHubRepo> GetRepoInfo(string repoName)
    {
        var url = $"https://api.github.com/repos/ardalis/{repoName}";
        var response = await _httpClient.GetStringAsync(url);
        return JsonSerializer.Deserialize<GitHubRepo>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    private class GitHubRepo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("stargazers_count")]
        public int StargazersCount { get; set; }
        
        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;
    }
}
