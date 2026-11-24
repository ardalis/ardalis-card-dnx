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
using Ardalis.Cli.Services;
using TimeWarp.Nuru;
using TimeWarp.Terminal;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Displays top videos from .NET Conf playlists using Nuru table widget.
/// </summary>
[NuruRoute("dotnetconf-score", Description = "Display top videos from .NET Conf playlists. Use --output <file.md> to save results as a markdown file.")]
[NuruRouteAlias("dotnetconf-stats")]
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

        /// <summary>
        /// Known .NET Conf playlists shipped with the CLI. These are merged with the remote
        /// playlists.json so new years work even before the website is updated, and they act
        /// as a fallback when the remote data can't be fetched.
        /// </summary>
        internal static readonly IReadOnlyList<PlaylistInfo> BuiltInPlaylists =
        [
            Playlist(2026, "PLJL9Ln_f6sGM", "https://www.youtube.com/watch?v=HK3IwBezg0w"),
            Playlist(2025, "PLdo4fOcmZ0oXtIlvq1tuORUtZqVG-HdCt", "https://www.youtube.com/watch?v=rjefnUC9Z90"),
            Playlist(2024, "PLdo4fOcmZ0oXeSG8BgCVru3zQtw_K4ANY", "https://www.youtube.com/watch?v=zw-ZtB1BNl8"),
            Playlist(2023, "PLdo4fOcmZ0oULyHSPBx-tQzePOYlhvrAU", "https://www.youtube.com/watch?v=yF9SwL0p0Y0"),
            Playlist(2022, "PLdo4fOcmZ0oVlqu_V8EXUDDnPsYwemxjn", "https://www.youtube.com/watch?v=j6u7Pw6dyUw"),
            Playlist(2021, "PLdo4fOcmZ0oVFtp9MDEBNbA2sSqYvXSXO", "https://www.youtube.com/watch?v=lkmvnjypENw"),
        ];

        private static PlaylistInfo Playlist(int year, string listId, string highlightUrl) => new()
        {
            Name = $".NET Conf {year}",
            Url = $"https://www.youtube.com/playlist?list={listId}",
            HighlightVideos = [new HighlightVideo { Url = highlightUrl }]
        };

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
                List<PlaylistInfo> remotePlaylists = await FetchPlaylistsDataAsync(ct);
                List<PlaylistInfo> playlists = MergePlaylists(remotePlaylists, BuiltInPlaylists);

                // Find matching playlist
                PlaylistInfo? playlist = playlists.FirstOrDefault(p => ExtractYear(p.Name) == year);

                if (playlist == null)
                {
                    terminal.WriteLine($"No playlist found for .NET Conf {year}".Yellow());
                    IEnumerable<int> availableYears = playlists
                        .Select(p => ExtractYear(p.Name))
                        .Where(y => y.HasValue)
                        .Select(y => y!.Value)
                        .Distinct();
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
            try
            {
                HttpClient webClient = httpClientFactory.CreateClient("ArdalisWeb");
                string response = await webClient.GetStringAsync(PlaylistsJsonUrl, ct);
                List<PlaylistInfo>? playlists = JsonSerializer.Deserialize<List<PlaylistInfo>>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return playlists?.Where(p => p is not null && !string.IsNullOrWhiteSpace(p.Name)).ToList() ?? [];
            }
            catch (Exception ex) when (ex is HttpRequestException or JsonException or TaskCanceledException && !ct.IsCancellationRequested)
            {
                // Remote data is optional; the built-in playlists are used instead.
                TimeWarpTerminal.Default.WriteLine("Could not load the latest playlist list from ardalis.com; using built-in playlists.".Gray());
                return [];
            }
        }

        /// <summary>
        /// Combines remote playlists with built-in ones. Remote entries take precedence for any
        /// year they define; built-in entries fill in years missing from the remote data.
        /// Results are ordered newest year first.
        /// </summary>
        internal static List<PlaylistInfo> MergePlaylists(
            IEnumerable<PlaylistInfo> remote,
            IEnumerable<PlaylistInfo> builtIn)
        {
            List<PlaylistInfo> remoteList = remote.ToList();
            HashSet<int> remoteYears = remoteList
                .Select(p => ExtractYear(p.Name))
                .Where(y => y.HasValue)
                .Select(y => y!.Value)
                .ToHashSet();

            return remoteList
                .Concat(builtIn.Where(p => ExtractYear(p.Name) is int y && !remoteYears.Contains(y)))
                .OrderByDescending(p => ExtractYear(p.Name) ?? 0)
                .ToList();
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
            Match match = Regex.Match(url, @"[?&]v=([^&#]+)");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            Match shortMatch = Regex.Match(url, @"youtu\.be/([^?&#/]+)");
            return shortMatch.Success ? shortMatch.Groups[1].Value : null;
        }

        internal static int? ExtractYear(string name)
        {
            Match match = Regex.Match(name, @"\b(20\d{2})\b");
            return match.Success && int.TryParse(match.Groups[1].Value, out int year) ? year : null;
        }
    }

    // JSON models for playlists.json
    internal sealed class PlaylistInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("highlight-videos")]
        public List<HighlightVideo> HighlightVideos { get; set; } = [];
    }

    internal sealed class HighlightVideo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
