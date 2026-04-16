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
            throw new InvalidOperationException("CosmosDb:DatabaseName missing from configuration.");

        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("CosmosDb:ContainerName missing from configuration.");

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
        _logger.LogInformation("Searching for products. Term: {SearchTerm}, MinCocoa: {MinCocoa}", searchTerm, minCocoa);

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

        _logger.LogInformation("Search completed. Found {Count} matches.", products.Count);
        return products;
    }

    public async Task AddAsync(Product product)
    {
        if (product.Id == 0)
        {
            product.Id = Math.Abs(Guid.NewGuid().GetHashCode());
        }

        _logger.LogInformation("Adding new product: {ProductName} to category {Category}.", product.Name, product.Category);

        try
        {
            var document = MapToDocument(product);
            await _container.CreateItemAsync(document, new PartitionKey(document.Category));
            _logger.LogInformation("Successfully created product with ID {ProductId}.", product.Id);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB error when adding product {ProductName}.", product.Name);
            throw;
        }
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        _logger.LogInformation("Fetching product by ID: {ProductId}", id);

        var query = new QueryDefinition("SELECT * FROM c WHERE c.Id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<CosmosProductDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var document = response.FirstOrDefault();

            if (document is not null)
            {
                return MapToProduct(document);
            }
        }

        _logger.LogWarning("Product with ID {ProductId} not found.", id);
        return null;
    }

    public async Task UpdateAsync(Product product)
    {
        var existingDocument = await GetDocumentByProductIdAsync(product.Id);

        if (existingDocument is null)
        {
            _logger.LogWarning("Update failed: Product with ID {ProductId} not found.", product.Id);
            return;
        }

        try 
        {
            var updatedDocument = MapToDocument(product, existingDocument.id);
            await _container.UpsertItemAsync(updatedDocument, new PartitionKey(updatedDocument.Category));
            _logger.LogInformation("Successfully updated product {ProductId} ({ProductName}).", product.Id, product.Name);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error occurred while updating product {ProductId} in Cosmos DB.", product.Id);
            throw;
        }
    }

    public async Task DeleteAsync(int id)
    {
        _logger.LogInformation("Attempting to delete product with ID: {ProductId}", id);
        
        var existingDocument = await GetDocumentByProductIdAsync(id);

        if (existingDocument is null)
        {
            _logger.LogWarning("Delete aborted: Product with ID {ProductId} was not found.", id);
            return;
        }

        try
        {
            await _container.DeleteItemAsync<CosmosProductDocument>(
                existingDocument.id,
                new PartitionKey(existingDocument.Category));
                
            _logger.LogInformation("Product with ID {ProductId} was successfully deleted from category {Category}.", id, existingDocument.Category);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Failed to delete product {ProductId} from Cosmos DB.", id);
            throw;
        }
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