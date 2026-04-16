using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using TheSnaxers.Models;
using Microsoft.Extensions.Logging;

namespace TheSnaxers.Repositories;

public class CosmosProductRepository : IProductRepository
{
    private readonly Container _container;
    private readonly ILogger<CosmosProductRepository> _logger;

    public CosmosProductRepository(
        CosmosClient cosmosClient, 
        IConfiguration configuration,
        ILogger<CosmosProductRepository> logger)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        var containerName = configuration["CosmosDb:ContainerName"];

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("CosmosDb:DatabaseName saknas i konfigurationen.");

        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("CosmosDb:ContainerName saknas i konfigurationen.");

        _container = cosmosClient.GetContainer(databaseName, containerName);
        _logger = logger;
    }

    public async Task<List<Product>> GetAllAsync()
    {
        _logger.LogInformation("Fetching all products from Cosmos DB.");

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<CosmosProductDocument>(query);
        var products = new List<Product>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            products.AddRange(response.Select(MapToProduct));
        }

        _logger.LogInformation("Successfully retrieved {ProductCount} products.", products.Count);
        return products;
    }

    public async Task<List<Product>> SearchAsync(string searchTerm, int? minCocoa)
    {
        var sql = "SELECT * FROM c WHERE 1 = 1";

        if (!string.IsNullOrWhiteSpace(searchTerm))
            sql += " AND CONTAINS(c.Name, @searchTerm, true)";

        if (minCocoa.HasValue)
            sql += " AND c.CocoaPercentage >= @minCocoa";

        var query = new QueryDefinition(sql);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.WithParameter("@searchTerm", searchTerm);

        if (minCocoa.HasValue)
            query = query.WithParameter("@minCocoa", minCocoa.Value);

        var iterator = _container.GetItemQueryIterator<CosmosProductDocument>(query);
        var products = new List<Product>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            products.AddRange(response.Select(MapToProduct));
        }

        return products;
    }

    public async Task AddAsync(Product product)
    {
        if (product.Id == 0)
        {
            product.Id = Math.Abs(Guid.NewGuid().GetHashCode());
        }

        _logger.LogInformation("Adding new product: {ProductName} to category {Category}.", product.Name, product.Category);

        var document = MapToDocument(product);
        await _container.CreateItemAsync(document, new PartitionKey(document.Category));
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.Id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<CosmosProductDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var document = response.FirstOrDefault();

            if (document is not null)
                return MapToProduct(document);
        }

        return null;
    }

    public async Task UpdateAsync(Product product)
    {
        var existingDocument = await GetDocumentByProductIdAsync(product.Id);

        if (existingDocument is null)
            return;

        var updatedDocument = MapToDocument(product, existingDocument.id);
        await _container.UpsertItemAsync(updatedDocument, new PartitionKey(updatedDocument.Category));
    }

    public async Task DeleteAsync(int id)
    {
        var existingDocument = await GetDocumentByProductIdAsync(id);

        if (existingDocument is null)
            return;

        await _container.DeleteItemAsync<CosmosProductDocument>(
            existingDocument.id,
            new PartitionKey(existingDocument.Category));
    }

    private async Task<CosmosProductDocument?> GetDocumentByProductIdAsync(int id)
    {
        var query = new QueryDefinition("SELECT * FROM c WHERE c.Id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<CosmosProductDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var document = response.FirstOrDefault();

            if (document is not null)
                return document;
        }

        return null;
    }

    private static Product MapToProduct(CosmosProductDocument doc)
    {
        return new Product
        {
            Id = doc.Id,
            Name = doc.Name,
            Brand = doc.Brand,
            CocoaPercentage = doc.CocoaPercentage,
            Country = doc.Country ?? "Okänt",
            Description = doc.Description,
            Price = doc.Price,
            Category = doc.Category,
            ImageUrl = doc.ImageUrl
        };
    }

    private static CosmosProductDocument MapToDocument(Product product, string? cosmosId = null)
    {
        return new CosmosProductDocument
        {
            id = cosmosId ?? Guid.NewGuid().ToString(),
            Id = product.Id,
            Name = product.Name,
            Brand = product.Brand,
            CocoaPercentage = product.CocoaPercentage,
            Country = product.Country ?? "Okänt",
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            ImageUrl = product.ImageUrl
        };
    }

    private class CosmosProductDocument
    {
        [JsonProperty("id")]
        public string id { get; set; } = string.Empty;

        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public int CocoaPercentage { get; set; }
        public string? Country { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }
}