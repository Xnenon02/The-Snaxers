using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using TheSnaxers.Services; // Dubbelkolla att detta matchar din källkod!

namespace TheSnaxers.Tests;

public class CountryServiceTests
{
    [Fact]
    public async Task GetCountryInfoAsync_ShouldReturnDefault_WhenCountryNameIsEmpty()
    {
        // Arrange
        var httpClient = new HttpClient(); 
        var service = new CountryService(httpClient);
        var emptyCountryName = "";

        // Act
        var result = await service.GetCountryInfoAsync(emptyCountryName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Okänt land", result.Name); 
    }
}