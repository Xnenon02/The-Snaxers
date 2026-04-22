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
        string databaseName,
        string containerName,
        ILogger<CosmosProductRepository> logger)
    {
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
            sql += " AND (CONTAINS(c.Name, @searchTerm, true) OR CONTAINS(c.Category, @searchTerm, true))";

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
        // Id sätts redan i Product-konstruktorn — ingen hash-logik behövs
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

    public async Task<Product?> GetByIdAsync(string id)
    {
        _logger.LogInformation("Fetching product by ID: {ProductId}", id);

        // Point-read — billig och snabb (1 RU), ingen query behövs
        var query = new QueryDefinition("SELECT * FROM c WHERE c.id = @id")
            .WithParameter("@id", id);

        var iterator = _container.GetItemQueryIterator<CosmosProductDocument>(query);

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            var document = response.FirstOrDefault();
            if (document is not null)
                return MapToProduct(document);
        }

        _logger.LogWarning("Product with ID {ProductId} not found.", id);
        return null;
    }

    public async Task UpdateAsync(Product product, string originalCategory)
    {
        _logger.LogInformation("Updating product {ProductId}. OriginalCategory: {OldCat} → NewCategory: {NewCat}",
            product.Id, originalCategory, product.Category);

        try
        {
            var document = MapToDocument(product);

            if (originalCategory == product.Category)
            {
                // Samma partition — direkt upsert utan förhämtning
                await _container.UpsertItemAsync(document, new PartitionKey(document.Category));
            }
            else
            {
                // Category byttes — partition key är immutable i Cosmos,
                // måste radera från gammal partition och skapa i ny
                _logger.LogInformation("Category ändrad — raderar från '{OldCat}' och skapar i '{NewCat}'",
                    originalCategory, product.Category);

                await _container.DeleteItemAsync<CosmosProductDocument>(
                    product.Id, new PartitionKey(originalCategory));

                await _container.CreateItemAsync(document, new PartitionKey(document.Category));
            }

            _logger.LogInformation("Successfully updated product {ProductId}.", product.Id);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error occurred while updating product {ProductId}.", product.Id);
            throw;
        }
    }

    public async Task DeleteAsync(string id, string category)
    {
        _logger.LogInformation("Attempting to delete product with ID: {ProductId}", id);

        try
        {
            // Direkt point-delete med känt id och partition key — inget förhämtningsanrop behövs (fix tech debt #2)
            await _container.DeleteItemAsync<CosmosProductDocument>(id, new PartitionKey(category));
            _logger.LogInformation("Product {ProductId} deleted.", id);
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Delete aborted: Product with ID {ProductId} was not found.", id);
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}.", id);
            throw;
        }
    }

    private static Product MapToProduct(CosmosProductDocument doc) => new()
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

    private static CosmosProductDocument MapToDocument(Product product) => new()
    {
        id = product.Id,
        Name = product.Name ?? string.Empty,
        Brand = product.Brand ?? string.Empty,
        CocoaPercentage = product.CocoaPercentage,
        Country = product.Country ?? "Okänt",
        Description = product.Description ?? string.Empty,
        Price = product.Price,
        Category = product.Category ?? string.Empty,
        ImageUrl = product.ImageUrl ?? string.Empty
    };

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