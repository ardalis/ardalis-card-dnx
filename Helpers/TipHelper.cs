using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ardalis.Helpers;

public static class TipHelper
{
    private const string TipsUrl = "https://ardalis.com/tips.json";
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(5)
    };
    private static readonly Random _random = new Random();

    private static readonly Tip FallbackTip = new Tip
    {
        TipText = "Always include units in non-Timespan time variables/properties (e.g. `int timeoutMilliseconds` not `int timeout`).",
        ReferenceLink = "https://ardalis.com/use-timespan-or-specify-units-in-duration-properties-and-parameters"
    };

    public static async Task<Tip> GetRandomTip()
    {
        try
        {
            var response = await _httpClient.GetStringAsync(TipsUrl);
            var tips = JsonSerializer.Deserialize<Tip[]>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            if (tips != null && tips.Length > 0)
            {
                return tips[_random.Next(tips.Length)];
            }
            
            return FallbackTip;
        }
        catch
        {
            return FallbackTip;
        }
    }

    public class Tip
    {
        [JsonPropertyName("tipText")]
        public string TipText { get; set; } = string.Empty;

        [JsonPropertyName("referenceLink")]
        public string ReferenceLink { get; set; } = string.Empty;
    }
}
