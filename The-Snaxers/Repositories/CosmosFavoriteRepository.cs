using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public class CosmosFavoriteRepository : IFavoriteRepository
{
    private readonly Container _container;
    private readonly Container _productsContainer;

    public CosmosFavoriteRepository(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        _container = cosmosClient.GetContainer(databaseName, "Favorites");
        _productsContainer = cosmosClient.GetContainer(databaseName, "Products");
    }

    public async Task<List<Favorite>> GetFavoritesByUserIdAsync(string userId)
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
            return new List<Favorite>();

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
                    Name = doc.Name,
                    Brand = doc.Brand,
                    CocoaPercentage = doc.CocoaPercentage,
                    Country = doc.Country,
                    Description = doc.Description,
                    Price = doc.Price,
                    Category = doc.Category,
                    ImageUrl = doc.ImageUrl
                };
            }
        }

        return cosmosFavorites.Select(f => new Favorite
        {
            UserId = f.UserId,
            ProductId = f.ProductId,
            SavedAt = f.SavedAt,
            Product = productMap.TryGetValue(f.ProductId, out var product) ? product : null
        }).ToList();
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
        var doc = new CosmosFavoriteDocument
        {
            id = Guid.NewGuid().ToString(),
            UserId = favorite.UserId,
            ProductId = favorite.ProductId,
            SavedAt = favorite.SavedAt
        };

        await _container.CreateItemAsync(doc, new PartitionKey(favorite.UserId));
    }

    public async Task RemoveAsync(string userId, int productId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.UserId = @userId AND c.ProductId = @productId")
            .WithParameter("@userId", userId)
            .WithParameter("@productId", productId);

        var iterator = _container.GetItemQueryIterator<CosmosDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            foreach (var doc in response)
            {
                await _container.DeleteItemAsync<CosmosDocument>(
                    doc.id, new PartitionKey(userId));
            }
        }
    }

    // Hjälpklass för deserialisering från Cosmos — ingen JsonProperty, PascalCase matchar /UserId
    private class CosmosFavorite
    {
        public string id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime SavedAt { get; set; }
    }

    // Hjälpklass för att läsa id vid borttagning
    private class CosmosDocument
    {
        public string id { get; set; } = string.Empty;
    }

    // Hjälpklass för att skriva favoriter till Cosmos — ingen JsonProperty, PascalCase matchar /UserId
    private class CosmosFavoriteDocument
    {
        public string id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime SavedAt { get; set; }
    }

    // Hjälpklass för deserialisering av produkter från Cosmos
    private class CosmosProductDocument
    {
        [JsonProperty("id")]
        public string id { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CocoaPercentage { get; set; }
        public string Country { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}