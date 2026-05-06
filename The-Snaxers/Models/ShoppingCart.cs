namespace TheSnaxers.Models
{
using Newtonsoft.Json;
public class ShoppingCart
{
    [JsonProperty("id")] // Cosmos vill ha ett småbokstavs-id
    public string Id { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty; // Kopplingen till din Identity-user
    
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
}