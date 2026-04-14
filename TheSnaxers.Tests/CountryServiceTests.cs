using Xunit;
using TheSnaxers.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace TheSnaxers.Tests;

public class CountryServiceTests
{
    [Fact]
    public async Task GetCountryInfoAsync_ShouldReturnDefault_WhenCountryNameIsEmpty()
    {
        // Arrange: Initialize service with a real HttpClient for now
        var httpClient = new HttpClient(); 
        var service = new CountryService(httpClient);
        var emptyCountryName = "";

        // Act: Call the service method
        var result = await service.GetCountryInfoAsync(emptyCountryName);

        // Assert: Verify that it returns the expected default values
        Assert.Equal("Okänt land", result.Name);
        Assert.Equal("", result.FlagUrl);
    }
}