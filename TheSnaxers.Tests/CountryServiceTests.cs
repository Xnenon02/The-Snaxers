using Xunit;
using System.Net.Http;
using System.Threading.Tasks;
using TheSnaxers.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace TheSnaxers.Tests;

public class CountryServiceTests
{
    // Helper to create a real in-memory cache for testing
    private static IMemoryCache CreateCache() =>
        new MemoryCache(new MemoryCacheOptions());

    [Fact]
    public async Task GetCountryInfoAsync_ShouldReturnDefault_WhenCountryNameIsEmpty()
    {
        // Arrange
        var httpClient = new HttpClient();
        var cache = CreateCache();
        var logger = NullLogger<CountryService>.Instance;
        var service = new CountryService(httpClient, cache, logger);
        var emptyCountryName = "";

        // Act
        var result = await service.GetCountryInfoAsync(emptyCountryName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Okänt land", result.Name);
    }
}