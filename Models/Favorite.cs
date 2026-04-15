using Newtonsoft.Json;

namespace TheSnaxers.Models;

public class Favorite
{
    [JsonProperty("id")]
    public int Id { get; set; }
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("productId")]
    public int ProductId { get; set; }
    
    [JsonProperty("savedAt")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation property - används ej i Cosmos
    public Product? Product { get; set; }
}