using System.Net.Http.Json;

namespace TheSnaxers.Services;

public class CountryService : ICountryService
{
    private readonly HttpClient _http;

    public CountryService(HttpClient http)
    {
        _http = http;
    }

    public async Task<CountryInfo> GetCountryInfoAsync(string countryName)
    {
        if (string.IsNullOrWhiteSpace(countryName))
           return new CountryInfo { Name = "Okänt land", FlagUrl = "" };

        try 
        {
            var response = await _http.GetFromJsonAsync<List<RestCountryResponse>>(
                $"https://restcountries.com/v3.1/name/{countryName}?fullText=true&fields=name,flags");

            var country = response?.FirstOrDefault();

            return new CountryInfo
            {
                Name = country?.Name?.Common ?? countryName,
                FlagUrl = country?.Flags?.Png ?? ""
            };
        }
        catch 
        {
            return new CountryInfo { Name = countryName, FlagUrl = "" };
        }
    }
}

public class RestCountryResponse {
    public CountryNameResponse Name { get; set; } = new();
    public CountryFlagResponse Flags { get; set; } = new();
}
public class CountryNameResponse { public string Common { get; set; } = string.Empty; }
public class CountryFlagResponse { public string Png { get; set; } = string.Empty; }