using Ardalis.Api;
using Ardalis.Cli.Telemetry;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class DotNetConfScoreCommand : AsyncCommand<DotNetConfScoreCommand.Settings>
{
    private readonly PostHogService _postHog;
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(30),
        BaseAddress = new Uri("https://api.ardalis.com/")
    };

    private const string PlaylistsJsonUrl = "https://ardalis.com/playlists.json";
    private const string EncodedPayload = "c2d3cTRzQlJMVUxyWThXOXcyeUdEcFY5aGRQSGNTS3ZHNHVwdjcwcmFDQQ==";

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[year]")]
        [Description("The .NET Conf year (e.g., 2025, 2024)")]
        public int Year { get; set; } = DateTime.Now.Year;
    }

    static DotNetConfScoreCommand()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    }

    public DotNetConfScoreCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand($"dotnetconf-score-{settings.Year}");

        AnsiConsole.MarkupLine($"[bold green].NET Conf {settings.Year} - Top Videos by Views[/]\n");

        try
        {
            // Fetch playlists data
            var playlists = await FetchPlaylistsData();

            // Find matching playlist
            var playlist = playlists.FirstOrDefault(p =>
                p.Name.Contains(settings.Year.ToString(), StringComparison.OrdinalIgnoreCase));

            if (playlist == null)
            {
                AnsiConsole.MarkupLine($"[yellow]No playlist found for .NET Conf {settings.Year}[/]");
                AnsiConsole.MarkupLine($"[dim]Available years: {string.Join(", ", playlists.Select(p => ExtractYear(p.Name)).Where(y => y.HasValue).Select(y => y.Value))}[/]");
                return 1;
            }

            var apiKey = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(EncodedPayload));
            var client = new ArdalisApiClient(_httpClient, apiKey);

            var topVideos = await client.GetTopVideosAsync(playlist.Url);

            // Get highlight video IDs
            var highlightVideoIds = playlist.HighlightVideos
                .Select(v => ExtractVideoId(v.Url))
                .Where(id => !string.IsNullOrEmpty(id))
                .ToHashSet();

            if (topVideos.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No videos found in the playlist[/]");
                return 1;
            }

            // Display in a table
            DisplayVideosTable(topVideos, highlightVideoIds, settings.Year);

            return 0;
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"[red]Error fetching data: {ex.Message}[/]\n");
            
            if (ex.Message.Contains("403"))
            {
                AnsiConsole.MarkupLine("[yellow]YouTube API returned 403 Forbidden. Please check:[/]");
                AnsiConsole.MarkupLine("  1. Your API key is valid");
                AnsiConsole.MarkupLine("  2. YouTube Data API v3 is enabled in your Google Cloud project");
                AnsiConsole.MarkupLine("     [link]https://console.cloud.google.com/apis/library/youtube.googleapis.com[/]");
                AnsiConsole.MarkupLine("  3. Your API key has the proper restrictions (or none for testing)");
                AnsiConsole.MarkupLine("  4. You haven't exceeded your daily quota");
            }
            
            return 1;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Unexpected error: {ex.Message}[/]");
            return 1;
        }
    }

    private static async Task<List<PlaylistInfo>> FetchPlaylistsData()
    {
        var response = await _httpClient.GetStringAsync(PlaylistsJsonUrl);
        var playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return playlists ?? new List<PlaylistInfo>();
    }

    private static void DisplayVideosTable(List<VideoDetails> videos, HashSet<string> highlightVideoIds, int year)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(new TableColumn("[bold]Rank[/]").Centered());
        table.AddColumn("[bold]Title[/]");
        table.AddColumn(new TableColumn("[bold]Views[/]").RightAligned());

        int rank = 1;
        foreach (var video in videos)
        {
            var isHighlighted = highlightVideoIds.Contains(video.Id);
            var title = video.Title.Length > 80 ? video.Title.Substring(0, 77) + "..." : video.Title;
            var views = video.ViewCount.ToString("N0");

            if (isHighlighted)
            {
                table.AddRow(
                    $"[bold yellow]{rank}[/]",
                    $"[bold yellow]⭐ [link={video.Url}]{title.EscapeMarkup()}[/][/]",
                    $"[bold yellow]{views}[/]"
                );
            }
            else
            {
                table.AddRow(
                    $"[dim]{rank}[/]",
                    $"[link={video.Url}]{title.EscapeMarkup()}[/]",
                    $"[dim]{views}[/]"
                );
            }

            rank++;
        }

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]⭐ indicates Ardalis's video[/]");
    }

    private static string ExtractVideoId(string url)
    {
        var match = Regex.Match(url, @"[?&]v=([^&]+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    private static int? ExtractYear(string name)
    {
        var match = Regex.Match(name, @"\b(20\d{2})\b");
        return match.Success && int.TryParse(match.Groups[1].Value, out var year) ? year : null;
    }

    // JSON models for playlists.json
    private class PlaylistInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("highlight-videos")]
        public List<HighlightVideo> HighlightVideos { get; set; } = new();
    }

    private class HighlightVideo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }

    // JSON models for YouTube API
    private class YouTubePlaylistResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubePlaylistItem> Items { get; set; } = new();

        [JsonPropertyName("nextPageToken")]
        public string NextPageToken { get; set; }
    }

    private class YouTubePlaylistItem
    {
        [JsonPropertyName("contentDetails")]
        public ContentDetails ContentDetails { get; set; } = new();
    }

    private class ContentDetails
    {
        [JsonPropertyName("videoId")]
        public string VideoId { get; set; } = string.Empty;
    }

    private class YouTubeVideosResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeVideo> Items { get; set; } = new();
    }

    private class YouTubeVideo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("snippet")]
        public VideoSnippet Snippet { get; set; } = new();

        [JsonPropertyName("statistics")]
        public VideoStatistics Statistics { get; set; } = new();
    }

    private class VideoSnippet
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }

    private class VideoStatistics
    {
        [JsonPropertyName("viewCount")]
        public string ViewCount { get; set; } = "0";
    }
}
