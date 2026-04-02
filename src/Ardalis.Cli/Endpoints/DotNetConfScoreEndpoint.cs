#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Api;
using TimeWarp.Nuru;
using TimeWarp.Terminal;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays top videos from .NET Conf playlists using Nuru table widget.
/// </summary>
[NuruRoute("dotnetconf-score", Description = "Display top videos from .NET Conf playlists. Use --output <file.md> to save results as a markdown file.")]
public sealed class DotNetConfScoreEndpoint : IQuery<Unit>
{
    [Parameter(Description = "Year to display scores for")]
    public int? Year { get; set; }

    [Option("output", "o", Description = "Write output to a markdown file (e.g. output.md)")]
    public string? Output { get; set; }

    public sealed class Handler(
        IHttpClientFactory httpClientFactory) : IQueryHandler<DotNetConfScoreEndpoint, Unit>
    {
        private const string PlaylistsJsonUrl = "https://ardalis.com/playlists.json";
        private const string EncodedPayload = "c2d3cTRzQlJMVUxyWThXOXcyeUdEcFY5aGRQSGNTS3ZHNHVwdjcwcmFDQQ==";

        private int GetDefaultYear() => DateTime.Now.Month >= 11 ? DateTime.Now.Year : DateTime.Now.Year - 1;
        public async ValueTask<Unit> Handle(DotNetConfScoreEndpoint query, CancellationToken ct)
        {
            int year = query.Year ?? GetDefaultYear();
            ITerminal terminal = TimeWarpTerminal.Default;

            terminal.WriteLine($".NET Conf {year} - Top Videos by Views".Green().Bold());
            terminal.WriteLine();
            terminal.WriteLine();

            try
            {
                // Fetch playlists data using ArdalisWeb client
                List<PlaylistInfo> playlists = await FetchPlaylistsDataAsync(ct);

                // Find matching playlist
                PlaylistInfo? playlist = playlists.FirstOrDefault(p =>
                    p.Name.Contains(year.ToString(), StringComparison.OrdinalIgnoreCase));

                if (playlist == null)
                {
                    terminal.WriteLine($"No playlist found for .NET Conf {year}".Yellow());
                    IEnumerable<int> availableYears = playlists
                        .Select(p => ExtractYear(p.Name))
                        .Where(y => y.HasValue)
                        .Select(y => y!.Value);
                    terminal.WriteLine($"Available years: {string.Join(", ", availableYears)}".Gray());
                    return default;
                }

                // Use ArdalisApi client for video stats
                HttpClient apiClient = httpClientFactory.CreateClient("ArdalisApi");
                string apiKey = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(EncodedPayload));
                var client = new ArdalisApiClient(apiClient, apiKey);

                List<VideoDetails> topVideos = await client.GetTopVideosAsync(playlist.Url);

                // Get highlight video IDs
                HashSet<string> highlightVideoIds = playlist.HighlightVideos
                    .Select(v => ExtractVideoId(v.Url))
                    .Where(id => !string.IsNullOrEmpty(id))
                    .ToHashSet()!;

                if (topVideos.Count == 0)
                {
                    terminal.WriteLine("No videos found in the playlist".Yellow());
                    return default;
                }

                // Display in a table or write to markdown file
                if (!string.IsNullOrWhiteSpace(query.Output))
                {
                    WriteMarkdownFile(topVideos, highlightVideoIds, year, query.Output);
                    terminal.WriteLine($"Markdown written to: {query.Output}".Green());
                }
                else
                {
                    DisplayVideosTable(topVideos, highlightVideoIds);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Error fetching .NET Conf data for year {year}");
                terminal.WriteLine($"Error fetching data: {ex.Message}".Red());
                terminal.WriteLine();

                if (ex.Message.Contains("403"))
                {
                    terminal.WriteLine("YouTube API returned 403 Forbidden. Please check:".Yellow());
                    terminal.WriteLine("  1. Your API key is valid");
                    terminal.WriteLine("  2. YouTube Data API v3 is enabled in your Google Cloud project");
                    terminal.WriteLine("     https://console.cloud.google.com/apis/library/youtube.googleapis.com");
                    terminal.WriteLine("  3. Your API key has the proper restrictions (or none for testing)");
                    terminal.WriteLine("  4. You haven't exceeded your daily quota");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                terminal.WriteLine($"Unexpected error: {ex.Message}".Red());
            }

            return default;
        }

        private async Task<List<PlaylistInfo>> FetchPlaylistsDataAsync(CancellationToken ct)
        {
            HttpClient webClient = httpClientFactory.CreateClient("ArdalisWeb");
            string response = await webClient.GetStringAsync(PlaylistsJsonUrl, ct);
            List<PlaylistInfo>? playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return playlists ?? [];
        }

        private static void WriteMarkdownFile(List<VideoDetails> videos, HashSet<string> highlightVideoIds, int year, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# .NET Conf {year} - Top Videos by Views");
            sb.AppendLine();
            sb.AppendLine("| Rank | Title | Views |");
            sb.AppendLine("|:----:|:------|------:|");

            int rank = 1;
            foreach (VideoDetails video in videos)
            {
                bool isHighlighted = highlightVideoIds.Contains(video.Id);
                string rawTitle = video.Title.Length > 80 ? video.Title[..77] + "..." : video.Title;
                string title = rawTitle.Replace("|", @"\|");
                string views = video.ViewCount.ToString("N0");
                string prefix = isHighlighted ? "⭐ " : string.Empty;
                sb.AppendLine($"| {rank} | {prefix}[{title}]({video.Url}) | {views} |");
                rank++;
            }

            sb.AppendLine();
            sb.AppendLine("*⭐ indicates Ardalis's video*");

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        private static void DisplayVideosTable(List<VideoDetails> videos, HashSet<string> highlightVideoIds)
        {
            var terminal = TimeWarpTerminal.Default;

            terminal.WriteTable(table =>
            {
                table
                    .AddColumn("Rank", Alignment.Center)
                    .AddColumn("Title", Alignment.Left)
                    .AddColumn("Views", Alignment.Right)
                    .Border(BorderStyle.Rounded);

                int rank = 1;
                foreach (VideoDetails video in videos)
                {
                    bool isHighlighted = highlightVideoIds.Contains(video.Id);
                    string title = video.Title.Length > 80 ? video.Title[..77] + "..." : video.Title;
                    string views = video.ViewCount.ToString("N0");

                    if (isHighlighted)
                    {
                        table.AddRow(
                            rank.ToString().Yellow().Bold(),
                            ("⭐ " + title).Link(video.Url).Yellow().Bold(),
                            views.Yellow().Bold()
                        );
                    }
                    else
                    {
                        table.AddRow(
                            rank.ToString().Gray(),
                            title.Link(video.Url),
                            views.Gray()
                        );
                    }

                    rank++;
                }
            });

            terminal.WriteLine();
            terminal.WriteLine("Links in the table can be opened in your browser by clicking on them.".Gray());
            terminal.WriteLine("⭐ indicates Ardalis's video".Gray());
        }

        private static string? ExtractVideoId(string url)
        {
            Match match = Regex.Match(url, @"[?&]v=([^&]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static int? ExtractYear(string name)
        {
            Match match = Regex.Match(name, @"\b(20\d{2})\b");
            return match.Success && int.TryParse(match.Groups[1].Value, out int year) ? year : null;
        }
    }

    // JSON models for playlists.json
    private sealed class PlaylistInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("highlight-videos")]
        public List<HighlightVideo> HighlightVideos { get; set; } = [];
    }

    private sealed class HighlightVideo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
