using System.Net.Http.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace TheSnaxers.Services;

public class CountryService : ICountryService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CountryService> _logger;

    // Cache duration: country data rarely changes, 24 hours is safe
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);

    public CountryService(HttpClient http, IMemoryCache cache, ILogger<CountryService> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CountryInfo> GetCountryInfoAsync(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
           return new CountryInfo { Name = "Okänt land", FlagUrl = "" };

        // Return cached result if available, avoiding redundant API calls per page load
        var cacheKey = $"country_{countryName.ToLower()}";
        if (_cache.TryGetValue(cacheKey, out CountryInfo? cached) && cached != null)
            return cached;

        try 
        {
            // Short timeout: fail fast instead of blocking the page for 21 seconds
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

            var response = await _http.GetFromJsonAsync<List<RestCountryResponse>>(
                $"https://restcountries.com/v3.1/name/{countryName}?fullText=true&fields=name,flags",
                cts.Token);

            var country = response?.FirstOrDefault();

            var result = new CountryInfo
            {
                Name = country?.Name?.Common ?? countryName,
                FlagUrl = country?.Flags?.Png ?? ""
            };

            // Store result in cache for future requests
            _cache.Set(cacheKey, result, CacheDuration);

            return result;
        }
        catch (OperationCanceledException)
        {
            // On failure, cache fallback briefly to avoid hammering a down API
            _logger.LogWarning("CountryService: Timeout fetching country info for {Country}", countryName);
            var fallback = new CountryInfo { Name = countryName, FlagUrl = "" };
            _cache.Set(cacheKey, fallback, TimeSpan.FromMinutes(5));
            return fallback;
        }
        catch (HttpRequestException ex)
        {
            // On failure, cache fallback briefly to avoid hammering a down API
            _logger.LogWarning(ex, "CountryService: Network error fetching country info for {Country}", countryName);
            var fallback = new CountryInfo { Name = countryName, FlagUrl = "" };
            _cache.Set(cacheKey, fallback, TimeSpan.FromMinutes(5));
            return fallback;
        }
        catch (Exception ex)
        {
            // On failure, cache fallback briefly to avoid hammering a down API
            _logger.LogError(ex, "CountryService: Unexpected error fetching country info for {Country}", countryName);
            var fallback = new CountryInfo { Name = countryName, FlagUrl = "" };
            _cache.Set(cacheKey, fallback, TimeSpan.FromMinutes(5));
            return fallback;
        }
    }
}

public class RestCountryResponse {
    public CountryNameResponse Name { get; set; } = new();
    public CountryFlagResponse Flags { get; set; } = new();
}
public class CountryNameResponse { public string Common { get; set; } = string.Empty; }
public class CountryFlagResponse { public string Png { get; set; } = string.Empty; }