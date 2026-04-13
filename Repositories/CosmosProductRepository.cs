using Microsoft.Azure.Cosmos;
using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public class CosmosProductRepository : IProductRepository
{
    private readonly Container _container;

    public CosmosProductRepository(CosmosClient cosmosClient, IConfiguration configuration)
    {
        var databaseName = configuration["CosmosDb:DatabaseName"];
        var containerName = configuration["CosmosDb:ContainerName"];

        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("CosmosDb:DatabaseName saknas i konfigurationen.");

        if (string.IsNullOrWhiteSpace(containerName))
            throw new InvalidOperationException("CosmosDb:ContainerName saknas i konfigurationen.");

        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<List<Product>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = _container.GetItemQueryIterator<Product>(query);
        var products = new List<Product>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            products.AddRange(response);
        }

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

        var iterator = _container.GetItemQueryIterator<Product>(query);
        var products = new List<Product>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            products.AddRange(response);
        }

        return products;
    }
}