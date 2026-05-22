using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Aviator.Main.Frontend;

public class PlanespottersService
{
    private static readonly HttpClient Http = new();
    private readonly ConcurrentDictionary<string, string?> _cache = new();

    public async Task<string?> GetPhotoUrlAsync(string registration)
    {
        if (string.IsNullOrWhiteSpace(registration)) return null;

        var key = registration.Trim().ToUpperInvariant();
        if (_cache.TryGetValue(key, out var cached)) return cached;

        try
        {
            var response = await Http.GetFromJsonAsync<PlanespottersResponse>(
                $"https://api.planespotters.net/pub/photos/reg/{Uri.EscapeDataString(key)}");

            var url = response?.Photos?.FirstOrDefault()?.ThumbnailLarge?.Src;
            _cache[key] = url;
            return url;
        }
        catch
        {
            _cache[key] = null;
            return null;
        }
    }
}

public class PlanespottersResponse
{
    [JsonPropertyName("photos")]
    public List<PlanespottersPhoto>? Photos { get; set; }
}

public class PlanespottersPhoto
{
    [JsonPropertyName("thumbnail_large")]
    public PlanespottersThumbnail? ThumbnailLarge { get; set; }

    [JsonPropertyName("link")]
    public string? Link { get; set; }
}

public class PlanespottersThumbnail
{
    [JsonPropertyName("src")]
    public string? Src { get; set; }
}
