using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TimeWarp.Nuru;

namespace Ardalis.Helpers;

public static class RecentHelper
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    // Define sources once to avoid duplication
    private static readonly List<(string Name, string Icon, Func<Task<List<RecentActivity>>> FetchFunc)> Sources = new()
    {
        ("Blog", "📝", FetchBlogPostsAsync),
        ("YouTube", "🎥", FetchYouTubeVideosAsync),
        ("GitHub", "⚡", FetchGitHubActivityAsync),
        ("Bluesky", "🦋", FetchBlueskyPostsAsync),
        ("LinkedIn", "💼", FetchLinkedInPostsAsync)
    };

    public static async Task<List<RecentActivity>> GetRecentActivitiesAsync(bool verbose = false)
    {
        if (verbose)
        {
            return await GetRecentActivitiesWithVerboseAsync();
        }

        // Fetch from all sources in parallel
        var tasks = Sources.Select(s => s.FetchFunc()).ToArray();

        var results = await Task.WhenAll(tasks);
        
        // Flatten and combine all results
        var allActivities = results.SelectMany(x => x).ToList();
        
        // Sort by date (most recent first) and take top 5
        return allActivities
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToList();
    }

    private static async Task<List<RecentActivity>> GetRecentActivitiesWithVerboseAsync()
    {
        ITerminal terminal = NuruTerminal.Default;
        var allActivities = new List<RecentActivity>();

        // Process each source and display results
        foreach (var source in Sources)
        {
            var displayName = $"{source.Icon} {source.Name}";
            
            try
            {
                var activities = await source.FetchFunc();
                
                if (activities.Count > 0)
                {
                    allActivities.AddRange(activities);
                    var resultText = activities.Count == 1 ? "result" : "results";
                    terminal.WriteLine($"{displayName}... ✅ {activities.Count} {resultText} found!".Gray());
                }
                else
                {
                    terminal.WriteLine($"{displayName}... ⚠️ No results found".Gray());
                }
            }
            catch (Exception ex)
            {
                // Extract just the error type for cleaner output
                var errorType = ex.GetType().Name;
                var errorMessage = ex.Message;
                
                // Try to extract HTTP status code if it's an HttpRequestException
                if (ex is HttpRequestException)
                {
                    if (errorMessage.Contains("404"))
                    {
                        terminal.WriteLine($"{displayName}... ❌ Request returned 404!".Gray());
                    }
                    else if (errorMessage.Contains("403"))
                    {
                        terminal.WriteLine($"{displayName}... ❌ Request returned 403 (Forbidden)!".Gray());
                    }
                    else if (errorMessage.Contains("500"))
                    {
                        terminal.WriteLine($"{displayName}... ❌ Request returned 500 (Server Error)!".Gray());
                    }
                    else
                    {
                        terminal.WriteLine($"{displayName}... ❌ Request failed: {errorType}!".Gray());
                    }
                }
                else if (ex is TaskCanceledException)
                {
                    terminal.WriteLine($"{displayName}... ❌ Request timed out!".Gray());
                }
                else
                {
                    terminal.WriteLine($"{displayName}... ❌ Error: {errorType}!".Gray());
                }
            }
        }

        terminal.WriteLine();
        
        // Sort by date (most recent first) and take top 5
        return allActivities
            .OrderByDescending(a => a.Date)
            .Take(5)
            .ToList();
    }

    private static async Task<List<RecentActivity>> FetchBlogPostsAsync()
    {
        try
        {
            var response = await _httpClient.GetStringAsync("https://ardalis.com/rss");
            var doc = XDocument.Parse(response);
            var ns = doc.Root?.GetDefaultNamespace();
            
            var items = doc.Descendants(ns + "item")
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
            
            return items;
        }
        catch
        {
            return new List<RecentActivity>();
        }
    }

    private static async Task<List<RecentActivity>> FetchYouTubeVideosAsync()
    {
        try
        {
            // YouTube RSS feed for channel
            var response = await _httpClient.GetStringAsync("https://www.youtube.com/feeds/videos.xml?channel_id=UCkvBKVrZ_RepwX7UgfnFIUA");
            var doc = XDocument.Parse(response);
            var ns = XNamespace.Get("http://www.w3.org/2005/Atom");
            var mediaNs = XNamespace.Get("http://search.yahoo.com/mrss/");
            
            var items = doc.Descendants(ns + "entry")
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
            
            return items;
        }
        catch
        {
            return new List<RecentActivity>();
        }
    }

    private static async Task<List<RecentActivity>> FetchGitHubActivityAsync()
    {
        try
        {
            // GitHub Atom feed for user activity
            var response = await _httpClient.GetStringAsync("https://github.com/ardalis.atom");
            var doc = XDocument.Parse(response);
            var ns = XNamespace.Get("http://www.w3.org/2005/Atom");
            
            var items = doc.Descendants(ns + "entry")
                .Take(5)
                .Select(entry => 
                {
                    var title = entry.Element(ns + "title")?.Value ?? "Untitled";
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
            
            return items;
        }
        catch
        {
            return new List<RecentActivity>();
        }
    }

    private static async Task<List<RecentActivity>> FetchBlueskyPostsAsync()
    {
        try
        {
            // Bluesky RSS bridge or public API
            // Note: Bluesky doesn't have a direct RSS feed, this is a simplified approach
            // In production, you'd use the Bluesky API with proper authentication
            var url = "https://bsky.app/profile/ardalis.com/rss";
            var response = await _httpClient.GetStringAsync(url);
            var doc = XDocument.Parse(response);
            var ns = doc.Root?.GetDefaultNamespace();
            
            var items = doc.Descendants(ns + "item")
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
            
            return items;
        }
        catch
        {
            return new List<RecentActivity>();
        }
    }

    private static async Task<List<RecentActivity>> FetchLinkedInPostsAsync()
    {
        // LinkedIn doesn't provide public RSS feeds
        // This is a placeholder - in production you'd need LinkedIn API access
        // For now, we'll return empty list as we can't fetch without API credentials
        await Task.CompletedTask; // Satisfy async requirement
        return new List<RecentActivity>();
    }

    private static DateTime ParseDate(string dateString)
    {
        if (string.IsNullOrEmpty(dateString))
            return DateTime.MinValue;
        
        if (DateTime.TryParse(dateString, out var date))
            return date;
        
        return DateTime.MinValue;
    }

    public class RecentActivity
    {
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;

        public string GetTruncatedTitle(int maxWidth)
        {
            if (Title.Length <= maxWidth)
                return Title;
            
            return Title[..(maxWidth - 3)] + "...";
        }

        public string GetRelativeTimeString()
        {
            var now = DateTime.UtcNow;
            var activityDate = Date.Kind == DateTimeKind.Utc ? Date : Date.ToUniversalTime();
            var timeSpan = now - activityDate;

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
                var hours = (int)timeSpan.TotalHours;
                return hours == 1 ? "1 hour ago" : $"{hours} hours ago";
            }
            
            // 1-2 days
            var days = (int)timeSpan.TotalDays;
            return days == 1 ? "1 day ago" : $"{days} days ago";
        }
    }
}
