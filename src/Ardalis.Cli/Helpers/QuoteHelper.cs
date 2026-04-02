using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ardalis.Helpers;

public static class QuoteHelper
{
    private const string QuotesUrl = "https://ardalis.com/quotes.json";
    private const string FallbackQuote = "New is glue.";
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(5)
    };
    private static readonly Random _random = new Random();

    public static async Task<string> GetRandomQuote()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(QuotesUrl);
            var quotes = System.Text.Json.JsonSerializer.Deserialize<string[]>(response);
            
            if (quotes != null && quotes.Length > 0)
            {
                return quotes[_random.Next(quotes.Length)];
            }
            
            return FallbackQuote;
        }
        catch
        {
            return FallbackQuote;
        }
    }
}
