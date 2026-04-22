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
        string databaseName,
        string favoritesContainerName,
        string productsContainerName,
        ILogger<CosmosFavoriteRepository> logger)
    {
        _container = cosmosClient.GetContainer(databaseName, favoritesContainerName);
        _productsContainer = cosmosClient.GetContainer(databaseName, productsContainerName);
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

            var cosmosFavorites = new List<CosmosFavoriteDocument>();
            var favIterator = _container.GetItemQueryIterator<CosmosFavoriteDocument>(favQuery);

            while (favIterator.HasMoreResults)
            {
                var response = await favIterator.ReadNextAsync();
                cosmosFavorites.AddRange(response);
            }

            if (!cosmosFavorites.Any())
                return new List<Favorite>();

            // Logga ProductIds för att underlätta felsökning av stale data
            var productIds = cosmosFavorites.Select(f => f.ProductId).ToList();
            _logger.LogInformation("Found {Count} favorites. Looking up ProductIds: {ProductIds}",
                cosmosFavorites.Count, string.Join(", ", productIds));

            var emptyIds = productIds.Where(string.IsNullOrWhiteSpace).ToList();
            if (emptyIds.Any())
                _logger.LogWarning("{EmptyCount} favorite(s) har tomt ProductId — troligen gammal data i Cosmos.", emptyIds.Count);

            // Hämta produkter i ett svep med IN-operatorn
            var idList = string.Join(",", cosmosFavorites.Select(f => $"'{f.ProductId}'"));
            var productQuery = new QueryDefinition(
                $"SELECT * FROM c WHERE c.id IN ({idList})");

            var productMap = new Dictionary<string, Product>();
            var productIterator = _productsContainer.GetItemQueryIterator<CosmosProductDocument>(productQuery);

            while (productIterator.HasMoreResults)
            {
                var response = await productIterator.ReadNextAsync();
                foreach (var doc in response)
                {
                    productMap[doc.id] = new Product
                    {
                        Id = doc.id,
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

            _logger.LogInformation("Product lookup complete. Found {FoundCount}/{TotalCount} products in map.",
                productMap.Count, productIds.Count);

            // Logga vilka ProductIds som inte matchade någon produkt
            var missingIds = productIds.Where(id => !string.IsNullOrWhiteSpace(id) && !productMap.ContainsKey(id)).ToList();
            if (missingIds.Any())
                _logger.LogWarning("Följande ProductIds hittades inte i Products-containern (stale data?): {MissingIds}",
                    string.Join(", ", missingIds));

            return cosmosFavorites.Select(f => new Favorite
            {
                Id = f.id,
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

    public async Task<bool> ExistsAsync(string userId, string productId)
    {
        var query = new QueryDefinition(
            "SELECT * FROM c WHERE c.UserId = @userId AND c.ProductId = @productId")
            .WithParameter("@userId", userId)
            .WithParameter("@productId", productId);

        var iterator = _container.GetItemQueryIterator<CosmosFavoriteDocument>(query);

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
            _logger.LogError(ex, "Failed to add favorite for user {UserId}", favorite.UserId);
            throw;
        }
    }

    public async Task RemoveAsync(string userId, string productId)
    {
        _logger.LogInformation("Removing product {ProductId} from favorites for user {UserId}", productId, userId);

        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.UserId = @userId AND c.ProductId = @productId")
                .WithParameter("@userId", userId)
                .WithParameter("@productId", productId);

            var iterator = _container.GetItemQueryIterator<CosmosFavoriteDocument>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var doc in response)
                {
                    await _container.DeleteItemAsync<CosmosFavoriteDocument>(
                        doc.id, new PartitionKey(userId));
                }
            }

            _logger.LogInformation("Removed favorite for user {UserId}, product {ProductId}", userId, productId);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error removing favorite for user {UserId}", userId);
            throw;
        }
    }

    private class CosmosFavoriteDocument
    {
        public string id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
    }

    private class CosmosProductDocument
    {
        [JsonProperty("id")]
        public string id { get; set; } = string.Empty;
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