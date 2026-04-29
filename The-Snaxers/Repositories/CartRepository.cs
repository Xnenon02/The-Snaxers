using Microsoft.Azure.Cosmos;
using TheSnaxers.Models;
using TheSnaxers.Repositories;
using TheSnaxers.Services;

public class CosmosCartRepository : ICartRepository
{
    private readonly Container _container;

    public CosmosCartRepository(CosmosClient cosmosClient, string databaseName, string containerName)
    {
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task<ShoppingCart> GetCartByUserIdAsync(string userId)
{
    try
    {
        // Ändra PartitionKey till userId (eftersom id och userId är samma i din modell)
        ItemResponse<ShoppingCart> response = await _container.ReadItemAsync<ShoppingCart>(userId, new PartitionKey(userId));
        return response.Resource;
    }
    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return new ShoppingCart { Id = userId, UserId = userId, Items = new List<CartItem>() };
    }
}

public async Task SaveCartAsync(ShoppingCart cart)
{
    // Ändra från cart.UserId till cart.Id här! 
    // I Products-containern är det 'id' som gäller som partition key.
    await _container.UpsertItemAsync(cart, new PartitionKey(cart.Id));
}
    public async Task ClearCartAsync(string userId)
{
    // I CosmosDB är det säkraste sättet att "tömma" korgen att 
    // helt enkelt spara en ny korg-instans som bara har en tom lista.
    var emptyCart = new ShoppingCart 
    { 
        Id = userId, 
        UserId = userId, 
        Items = new List<CartItem>() 
    };
    
    await SaveCartAsync(emptyCart);
}
}