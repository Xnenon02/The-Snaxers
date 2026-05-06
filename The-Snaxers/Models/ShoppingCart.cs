namespace TheSnaxers.Models
{
using System.Text.Json.Serialization;
using Newtonsoft.Json;

public class ShoppingCart
{
    // Both attribute types are required:
    // [JsonProperty]     = Newtonsoft.Json  — used by CosmosDB SDK v3 default serializer
    // [JsonPropertyName] = System.Text.Json — used if SDK is configured with STJ serializer
    // "id" must be lowercase (CosmosDB requirement)
    [JsonProperty("id")]
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    // "userId" must match the container's /userId partition key path exactly
    // Id must always be set explicitly to userId — Cosmos DB requires it to match the /userId partition key
    [JsonProperty("userId")]
    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty; // Kopplingen till din Identity-user

    public List<CartItem> Items { get; set; } = new List<CartItem>();

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
}
