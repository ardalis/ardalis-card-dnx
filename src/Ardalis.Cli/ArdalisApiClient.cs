// ArdalisApiClient.cs
// Self-contained client for api.ardalis.com
// Copy this entire file into your project and use the ArdalisApiClient class
// 
// Usage:
//   var client = new ArdalisApiClient("your-api-key", "https://api.ardalis.com");
//   var stats = await client.GetPlaylistStatsAsync("PLsmmv2FZDrU9rLQ0s1eMVqQhGVvRWGbHH");
//   foreach (var video in stats.Videos)
//   {
//       Console.WriteLine($"{video.Title}: {video.ViewCount:N0} views");
//   }

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Ardalis.Api;

/// <summary>
/// Client for interacting with api.ardalis.com
/// </summary>
public class ArdalisApiClient : IDisposable
{
  private readonly HttpClient _httpClient;
  private readonly bool _ownsHttpClient;
  private readonly string _apiKey;

  /// <summary>
  /// Creates a new instance of ArdalisApiClient with an internal HttpClient
  /// </summary>
  /// <param name="apiKey">API key for authentication</param>
  /// <param name="baseUrl">Base URL of the API (default: https://api.ardalis.com)</param>
  public ArdalisApiClient(string apiKey, string baseUrl = "https://api.ardalis.com")
  {
    if (string.IsNullOrWhiteSpace(apiKey))
      throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

    _apiKey = apiKey;
    _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    _ownsHttpClient = true;
  }

  /// <summary>
  /// Creates a new instance of ArdalisApiClient with a provided HttpClient
  /// </summary>
  /// <param name="httpClient">HttpClient to use for requests</param>
  /// <param name="apiKey">API key for authentication</param>
  public ArdalisApiClient(HttpClient httpClient, string apiKey)
  {
    if (string.IsNullOrWhiteSpace(apiKey))
      throw new ArgumentException("API key cannot be null or empty", nameof(apiKey));

    _apiKey = apiKey;
    _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    _httpClient.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    _ownsHttpClient = false;
  }

  /// <summary>
  /// Gets statistics for a YouTube playlist ordered by view count (most popular first)
  /// </summary>
  /// <param name="playlistIdOrUrl">YouTube playlist ID or full URL</param>
  /// <param name="youtubeApiKeyOverride">Optional YouTube API key to override server's default</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Playlist statistics with video details</returns>
  public async Task<PlaylistStatsResponse> GetPlaylistStatsAsync(
    string playlistIdOrUrl,
    string? youtubeApiKeyOverride = null,
    CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(playlistIdOrUrl))
      throw new ArgumentException("Playlist ID or URL cannot be null or empty", nameof(playlistIdOrUrl));

    var playlistId = ExtractPlaylistId(playlistIdOrUrl);

    using var request = new HttpRequestMessage(HttpMethod.Get, $"/youtube/playlists/{Uri.EscapeDataString(playlistId)}/stats");
    
    if (!string.IsNullOrWhiteSpace(youtubeApiKeyOverride))
    {
      request.Headers.Add("X-YouTube-Api-Key", youtubeApiKeyOverride);
    }

    var response = await _httpClient.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync(cancellationToken);
      throw new ArdalisApiException(
        $"Failed to get playlist stats. Status: {response.StatusCode}. Error: {error}",
        response.StatusCode);
    }

    var result = await response.Content.ReadFromJsonAsync<PlaylistStatsResponse>(cancellationToken);
    return result ?? throw new ArdalisApiException("Failed to deserialize response", System.Net.HttpStatusCode.InternalServerError);
  }

  /// <summary>
  /// Lists videos with pagination support
  /// </summary>
  /// <param name="page">Page number (1-based, default: 1)</param>
  /// <param name="perPage">Items per page (1-100, default: 50)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Paginated list of videos</returns>
  public async Task<VideoListResponse> ListVideosAsync(
    int page = 1,
    int perPage = 50,
    CancellationToken cancellationToken = default)
  {
    if (page < 1)
      throw new ArgumentException("Page must be >= 1", nameof(page));
    if (perPage < 1 || perPage > 100)
      throw new ArgumentException("PerPage must be between 1 and 100", nameof(perPage));

    var response = await _httpClient.GetAsync($"/Videos?page={page}&per_page={perPage}", cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync(cancellationToken);
      throw new ArdalisApiException(
        $"Failed to list videos. Status: {response.StatusCode}. Error: {error}",
        response.StatusCode);
    }

    var result = await response.Content.ReadFromJsonAsync<VideoListResponse>(cancellationToken);
    return result ?? throw new ArdalisApiException("Failed to deserialize response", System.Net.HttpStatusCode.InternalServerError);
  }

  /// <summary>
  /// Extracts a YouTube playlist ID from either a playlist ID string or a full YouTube playlist URL
  /// </summary>
  private static string ExtractPlaylistId(string playlistIdOrUrl)
  {
    // If it's already just an ID (no special characters like ?, &, =), return it
    if (!playlistIdOrUrl.Contains('?') && !playlistIdOrUrl.Contains('&') && !playlistIdOrUrl.Contains('='))
    {
      return playlistIdOrUrl.Trim();
    }

    // Try to extract from URL
    var match = Regex.Match(playlistIdOrUrl, @"[?&]list=([^&]+)");
    return match.Success ? match.Groups[1].Value : playlistIdOrUrl.Trim();
  }

  public void Dispose()
  {
    if (_ownsHttpClient)
    {
      _httpClient.Dispose();
    }
  }
}

/// <summary>
/// Exception thrown when an API call fails
/// </summary>
public class ArdalisApiException : Exception
{
  public System.Net.HttpStatusCode StatusCode { get; }

  public ArdalisApiException(string message, System.Net.HttpStatusCode statusCode)
    : base(message)
  {
    StatusCode = statusCode;
  }

  public ArdalisApiException(string message, System.Net.HttpStatusCode statusCode, Exception innerException)
    : base(message, innerException)
  {
    StatusCode = statusCode;
  }
}

// Response Types

/// <summary>
/// Response containing YouTube playlist statistics
/// </summary>
public record PlaylistStatsResponse(List<VideoDetails> Videos);

/// <summary>
/// Details about a YouTube video
/// </summary>
public record VideoDetails(
  string Id,
  string Title,
  DateTime PublishedAt,
  long ViewCount,
  string Url);

/// <summary>
/// Paginated response containing videos
/// </summary>
public record VideoListResponse(
  List<VideoRecord> Items,
  int Page,
  int PerPage,
  int TotalCount,
  int TotalPages);

/// <summary>
/// Video record with full details
/// </summary>
public record VideoRecord(
  Guid Id,
  string YouTubeVideoId,
  string Title,
  DateTime PublishedAt,
  long ViewCount,
  string Url);

// Extension methods for convenience

/// <summary>
/// Extension methods for ArdalisApiClient
/// </summary>
public static class ArdalisApiClientExtensions
{
  /// <summary>
  /// Gets all videos from a playlist (handles pagination automatically)
  /// </summary>
  public static async Task<List<VideoRecord>> GetAllVideosAsync(
    this ArdalisApiClient client,
    CancellationToken cancellationToken = default)
  {
    var allVideos = new List<VideoRecord>();
    int page = 1;
    const int perPage = 100; // Max page size

    while (true)
    {
      var response = await client.ListVideosAsync(page, perPage, cancellationToken);
      allVideos.AddRange(response.Items);

      if (page >= response.TotalPages)
        break;

      page++;
    }

    return allVideos;
  }

  /// <summary>
  /// Gets the most viewed videos from a playlist
  /// </summary>
  public static async Task<List<VideoDetails>> GetTopVideosAsync(
    this ArdalisApiClient client,
    string playlistIdOrUrl,
    int count = 10,
    CancellationToken cancellationToken = default)
  {
    var stats = await client.GetPlaylistStatsAsync(playlistIdOrUrl, cancellationToken: cancellationToken);
    return stats.Videos.Take(count).ToList();
  }
}
