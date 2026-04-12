namespace TheSnaxers.Services;

public interface ICountryService
{
    // En metod som tar ett landsnamn och returnerar info (namn + flagga)
    Task<CountryInfo> GetCountryInfoAsync(string countryName);
}

public class CountryInfo 
{
    public string Name { get; set; } = "Unknown";
    public string FlagUrl { get; set; } = "/images/flag-placeholder.png";
}