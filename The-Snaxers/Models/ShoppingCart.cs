namespace TheSnaxers.Models
{
using System.Text.Json.Serialization;
public class ShoppingCart
{
    // CosmosDB SDK v3 uses System.Text.Json — [JsonPropertyName] ensures correct casing
    // "id" must be lowercase (CosmosDB requirement)
    // "userId" must match the container's /userId partition key path exactly
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    // Id must always be set explicitly to userId — Cosmos DB requires it to match the /userId partition key
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty; // Kopplingen till din Identity-user

    public List<CartItem> Items { get; set; } = new List<CartItem>();

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
}
