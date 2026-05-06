namespace TheSnaxers.Models
{
using Newtonsoft.Json;
public class ShoppingCart
{
    [JsonProperty("id")] // Cosmos vill ha ett småbokstavs-id
    
    // Id must always be set explicitly to userId — Cosmos DB requires it to match the /userId partition key
    public string Id { get; set; } = string.Empty;

    [JsonProperty("userId")] // Cosmos partition key /userId is case-sensitive — must match exactly
    public string UserId { get; set; } = string.Empty; // Kopplingen till din Identity-user
    
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
}