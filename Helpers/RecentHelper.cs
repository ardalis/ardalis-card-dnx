using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ardalis.Helpers;

public static class RecentHelper
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    public static async Task<List<RecentActivity>> GetRecentActivitiesAsync()
    {
        // Fetch from all sources in parallel
        var tasks = new[]
        {
            FetchBlogPostsAsync(),
            FetchYouTubeVideosAsync(),
            FetchGitHubActivityAsync(),
            FetchBlueskyPostsAsync(),
            FetchLinkedInPostsAsync()
        };

        var results = await Task.WhenAll(tasks);
        
        // Flatten and combine all results
        var allActivities = results.SelectMany(x => x).ToList();
        
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
            var response = await _httpClient.GetStringAsync("https://ardalis.com/index.xml");
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
                        Icon = ""
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
                    Title = item.Element(ns + "title")?.Value ?? "Untitled",
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
    }
}
