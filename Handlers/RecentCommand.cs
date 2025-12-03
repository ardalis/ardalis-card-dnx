#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Mediator;
using Microsoft.Extensions.Logging;
using TimeWarp.Nuru;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli.Handlers;

/// <summary>
/// Displays recent activity from various sources using Nuru table widget.
/// </summary>
public sealed class RecentCommand : IRequest
{
    public bool Verbose { get; init; }

    public sealed class Handler : IRequestHandler<RecentCommand>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<Handler> _logger;

        private static readonly List<(string Name, string Icon, string Url, Func<XDocument, List<RecentActivity>> Parser)> Sources =
        [
            ("Blog", "📝", "https://ardalis.com/rss", ParseBlogRss),
            ("YouTube", "🎥", "https://www.youtube.com/feeds/videos.xml?channel_id=UCkvBKVrZ_RepwX7UgfnFIUA", ParseYouTubeAtom),
            ("GitHub", "⚡", "https://github.com/ardalis.atom", ParseGitHubAtom),
            ("Bluesky", "🦋", "https://bsky.app/profile/ardalis.com/rss", ParseBlueskyRss)
        ];

        public Handler(
            IHttpClientFactory httpClientFactory,
            ILogger<Handler> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async ValueTask<Unit> Handle(
            RecentCommand request,
            CancellationToken cancellationToken)
        {
            ITerminal terminal = NuruTerminal.Default;

            terminal.WriteLine("Fetching recent activity...".Bold());
            terminal.WriteLine();

            List<RecentActivity> activities = request.Verbose
                ? await GetRecentActivitiesWithVerboseAsync(terminal, cancellationToken)
                : await GetRecentActivitiesAsync(cancellationToken);

            if (activities.Count == 0)
            {
                terminal.WriteLine("No recent activities found.".Yellow());
                return Unit.Value;
            }

            DisplayTable(terminal, activities);

            return Unit.Value;
        }

        private async Task<List<RecentActivity>> GetRecentActivitiesAsync(CancellationToken cancellationToken)
        {
            HttpClient client = _httpClientFactory.CreateClient("RssFeed");

            var tasks = Sources.Select(async source =>
            {
                try
                {
                    string response = await client.GetStringAsync(source.Url, cancellationToken);
                    XDocument doc = XDocument.Parse(response);
                    return source.Parser(doc);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch {SourceName}", source.Name);
                    return new List<RecentActivity>();
                }
            }).ToArray();

            var results = await Task.WhenAll(tasks);

            return results
                .SelectMany(x => x)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList();
        }

        private async Task<List<RecentActivity>> GetRecentActivitiesWithVerboseAsync(
            ITerminal terminal,
            CancellationToken cancellationToken)
        {
            HttpClient client = _httpClientFactory.CreateClient("RssFeed");
            var allActivities = new List<RecentActivity>();

            foreach (var source in Sources)
            {
                string displayName = $"{source.Icon} {source.Name}";

                try
                {
                    string response = await client.GetStringAsync(source.Url, cancellationToken);
                    XDocument doc = XDocument.Parse(response);
                    List<RecentActivity> activities = source.Parser(doc);

                    if (activities.Count > 0)
                    {
                        allActivities.AddRange(activities);
                        string resultText = activities.Count == 1 ? "result" : "results";
                        terminal.WriteLine($"{displayName}... ✅ {activities.Count} {resultText} found!".Gray());
                    }
                    else
                    {
                        terminal.WriteLine($"{displayName}... ⚠️ No results found".Gray());
                    }
                }
                catch (HttpRequestException ex)
                {
                    string errorMessage = ex.Message;
                    if (errorMessage.Contains("404"))
                        terminal.WriteLine($"{displayName}... ❌ Request returned 404!".Gray());
                    else if (errorMessage.Contains("403"))
                        terminal.WriteLine($"{displayName}... ❌ Request returned 403 (Forbidden)!".Gray());
                    else if (errorMessage.Contains("500"))
                        terminal.WriteLine($"{displayName}... ❌ Request returned 500 (Server Error)!".Gray());
                    else
                        terminal.WriteLine($"{displayName}... ❌ Request failed: HttpRequestException!".Gray());

                    _logger.LogWarning(ex, "Failed to fetch {SourceName}", source.Name);
                }
                catch (TaskCanceledException ex)
                {
                    terminal.WriteLine($"{displayName}... ❌ Request timed out!".Gray());
                    _logger.LogWarning(ex, "Timeout fetching {SourceName}", source.Name);
                }
                catch (Exception ex)
                {
                    string errorType = ex.GetType().Name;
                    terminal.WriteLine($"{displayName}... ❌ Error: {errorType}!".Gray());
                    _logger.LogWarning(ex, "Failed to fetch {SourceName}", source.Name);
                }
            }

            terminal.WriteLine();

            return allActivities
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList();
        }

        private static List<RecentActivity> ParseBlogRss(XDocument doc)
        {
            XNamespace? ns = doc.Root?.GetDefaultNamespace();
            if (ns == null) return [];

            return doc.Descendants(ns + "item")
                .Take(5)
                .Select(item => new RecentActivity
                {
                    Title = item.Element(ns + "title")?.Value ?? "Untitled",
                    Url = item.Element(ns + "link")?.Value ?? "",
                    Date = ParseDate(item.Element(ns + "pubDate")?.Value),
                    Source = "Blog",
                    Icon = "📝"
                })
                .ToList();
        }

        private static List<RecentActivity> ParseYouTubeAtom(XDocument doc)
        {
            XNamespace ns = XNamespace.Get("http://www.w3.org/2005/Atom");

            return doc.Descendants(ns + "entry")
                .Take(5)
                .Select(entry => new RecentActivity
                {
                    Title = entry.Element(ns + "title")?.Value ?? "Untitled",
                    Url = entry.Element(ns + "link")?.Attribute("href")?.Value ?? "",
                    Date = ParseDate(entry.Element(ns + "published")?.Value),
                    Source = "YouTube",
                    Icon = "🎥"
                })
                .ToList();
        }

        private static List<RecentActivity> ParseGitHubAtom(XDocument doc)
        {
            XNamespace ns = XNamespace.Get("http://www.w3.org/2005/Atom");

            return doc.Descendants(ns + "entry")
                .Take(5)
                .Select(entry =>
                {
                    string title = entry.Element(ns + "title")?.Value ?? "Untitled";
                    // Clean up the title to remove username prefix
                    title = Regex.Replace(title, @"^ardalis\s+", "");

                    return new RecentActivity
                    {
                        Title = title,
                        Url = entry.Element(ns + "link")?.Attribute("href")?.Value ?? "",
                        Date = ParseDate(entry.Element(ns + "updated")?.Value),
                        Source = "GitHub",
                        Icon = "⚡"
                    };
                })
                .ToList();
        }

        private static List<RecentActivity> ParseBlueskyRss(XDocument doc)
        {
            XNamespace? ns = doc.Root?.GetDefaultNamespace();
            if (ns == null) return [];

            return doc.Descendants(ns + "item")
                .Take(5)
                .Select(item => new RecentActivity
                {
                    Title = item.Element(ns + "description")?.Value ?? "Untitled",
                    Url = item.Element(ns + "link")?.Value ?? "https://bsky.app/profile/ardalis.com",
                    Date = ParseDate(item.Element(ns + "pubDate")?.Value),
                    Source = "Bluesky",
                    Icon = "🦋"
                })
                .ToList();
        }

        private static DateTime ParseDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return DateTime.MinValue;

            if (DateTime.TryParse(dateString, out DateTime date))
                return date;

            return DateTime.MinValue;
        }

        private static void DisplayTable(ITerminal terminal, List<RecentActivity> activities)
        {
            Table table = new Table()
                .AddColumn("Source", Alignment.Center)
                .AddColumn("Activity", Alignment.Left)
                .AddColumn("When", Alignment.Right)
                .AddColumn("Link", Alignment.Center);

            table.Border = BorderStyle.Rounded;

            foreach (RecentActivity activity in activities)
            {
                string truncatedTitle = activity.GetTruncatedTitle(60);
                string sourceWithIcon = $"{activity.Icon} {activity.Source}";
                string when = activity.GetRelativeTimeString();

                // Add UTM tracking to URL
                string urlWithTracking = AddUtmSource(activity.Url);
                string link = "Click for details".Link(urlWithTracking).Cyan();

                table.AddRow(sourceWithIcon, truncatedTitle, when, link);
            }

            terminal.WriteTable(table);
        }
    }

    private sealed class RecentActivity
    {
        public string Title { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public DateTime Date { get; init; }
        public string Source { get; init; } = string.Empty;
        public string Icon { get; init; } = string.Empty;

        public string GetTruncatedTitle(int maxWidth)
        {
            if (Title.Length <= maxWidth)
                return Title;

            return Title[..(maxWidth - 3)] + "...";
        }

        public string GetRelativeTimeString()
        {
            DateTime now = DateTime.UtcNow;
            DateTime activityDate = Date.Kind == DateTimeKind.Utc ? Date : Date.ToUniversalTime();
            TimeSpan timeSpan = now - activityDate;

            // If more than 2 days old, use short date format
            if (timeSpan.TotalDays > 2)
            {
                return activityDate.ToString("d MMM yyyy");
            }

            // Otherwise use relative time
            if (timeSpan.TotalMinutes < 1)
                return "just now";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} min ago";
            if (timeSpan.TotalHours < 24)
            {
                int hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }

            // 1-2 days
            int days = (int)timeSpan.TotalDays;
            return days == 1 ? "1 day ago" : $"{days} days ago";
        }
    }
}
