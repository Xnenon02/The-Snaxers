using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using TheSnaxers.Models;
using Microsoft.Extensions.Logging;

namespace TheSnaxers.Repositories;

public class CosmosFavoriteRepository : IFavoriteRepository
{
    private readonly Container _container;
    private readonly Container _productsContainer;
    private readonly ILogger<CosmosFavoriteRepository> _logger;

    public CosmosFavoriteRepository(
        CosmosClient cosmosClient, 
        IConfiguration configuration,
        ILogger<CosmosFavoriteRepository> logger)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = cosmosClient.GetContainer(databaseName, "Favorites");
        _productsContainer = cosmosClient.GetContainer(databaseName, "Products");
        _logger = logger;
    }

    public async Task<List<Favorite>> GetFavoritesByUserIdAsync(string userId)
    {
        _logger.LogInformation("Fetching favorites for user: {UserId}", userId);

        try
        {
            var favQuery = new QueryDefinition(
                "SELECT * FROM c WHERE c.UserId = @userId")
                .WithParameter("@userId", userId);

            var cosmosFavorites = new List<CosmosFavorite>();
            var favIterator = _container.GetItemQueryIterator<CosmosFavorite>(favQuery);

            while (favIterator.HasMoreResults)
            {
                var response = await favIterator.ReadNextAsync();
                cosmosFavorites.AddRange(response);
            }

            if (!cosmosFavorites.Any())
            {
                _logger.LogInformation("No favorites found for user: {UserId}", userId);
                return new List<Favorite>();
            }

            // Hämta tillhörande produkter
            var productIds = string.Join(",", cosmosFavorites.Select(f => f.ProductId));
            var productQuery = new QueryDefinition(
                $"SELECT * FROM c WHERE c.Id IN ({productIds})");

            var productMap = new Dictionary<int, Product>();
            var productIterator = _productsContainer.GetItemQueryIterator<CosmosProductDocument>(productQuery);

            while (productIterator.HasMoreResults)
            {
                var response = await productIterator.ReadNextAsync();
                foreach (var doc in response)
                {
                    productMap[doc.Id] = new Product
                    {
                        Id = doc.Id,
                        Name = doc.Name ?? string.Empty,
                        Brand = doc.Brand ?? string.Empty,
                        CocoaPercentage = doc.CocoaPercentage,
                        Country = doc.Country ?? "Okänt",
                        Description = doc.Description ?? string.Empty,
                        Price = doc.Price,
                        Category = doc.Category ?? string.Empty,
                        ImageUrl = doc.ImageUrl ?? string.Empty
                    };
                }
            }

            _logger.LogInformation("Successfully retrieved {Count} favorites with product details for user {UserId}", cosmosFavorites.Count, userId);

            return cosmosFavorites.Select(f => new Favorite
            {
                UserId = f.UserId ?? string.Empty,
                ProductId = f.ProductId,
                SavedAt = f.SavedAt,
                Product = productMap.TryGetValue(f.ProductId, out var product) ? product : null
            }).ToList();
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error while fetching favorites for user {UserId}", userId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string userId, int productId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.UserId = @userId AND c.ProductId = @productId")
            .WithParameter("@userId", userId)
            .WithParameter("@productId", productId);

        var iterator = _container.GetItemQueryIterator<CosmosFavorite>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            if (response.Any()) return true;
        }

        return false;
    }

    public async Task AddAsync(Favorite favorite)
    {
        _logger.LogInformation("Adding product {ProductId} to favorites for user {UserId}", favorite.ProductId, favorite.UserId);

        try
        {
            var doc = new CosmosFavoriteDocument
            {
                id = Guid.NewGuid().ToString(),
                UserId = favorite.UserId ?? string.Empty,
                ProductId = favorite.ProductId,
                SavedAt = favorite.SavedAt
            };

            await _container.CreateItemAsync(doc, new PartitionKey(doc.UserId));
            _logger.LogInformation("Successfully added favorite for user {UserId}", favorite.UserId);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to add favorite for user {UserId} and product {ProductId}", favorite.UserId, favorite.ProductId);
            throw;
        }
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        _logger.LogInformation("Attempting to remove product {ProductId} from favorites for user {UserId}", productId, userId);

        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.UserId = @userId AND c.ProductId = @productId")
                .WithParameter("@userId", userId)
                .WithParameter("@productId", productId);

            var iterator = _container.GetItemQueryIterator<CosmosDocument>(query);
            int deleteCount = 0;

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var doc in response)
                {
                    await _container.DeleteItemAsync<CosmosDocument>(
                        doc.id, new PartitionKey(userId));
                    deleteCount++;
                }
            }

            _logger.LogInformation("Removed {Count} favorite entry for user {UserId}, product {ProductId}", deleteCount, userId, productId);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error removing favorite for user {UserId}", userId);
            throw;
        }
    }

    private class CosmosFavorite
    {
        public string id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime SavedAt { get; set; }
    }

    private class CosmosDocument
    {
        public string id { get; set; } = string.Empty;
    }

    private class CosmosFavoriteDocument
    {
        public string id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime SavedAt { get; set; }
    }

    private class CosmosProductDocument
    {
        [JsonProperty("id")]
        public string id { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CocoaPercentage { get; set; }
        public string Country { get; set; } = "Okänt";
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}