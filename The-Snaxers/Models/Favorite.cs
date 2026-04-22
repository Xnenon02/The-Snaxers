using Newtonsoft.Json;

namespace TheSnaxers.Models;

public class Favorite
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [JsonProperty("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [JsonProperty("productId")]
    public string ProductId { get; set; } = string.Empty;
    
    [JsonProperty("savedAt")]
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    public Product? Product { get; set; }
}