namespace TheSnaxers.Models
{
    using Newtonsoft.Json;
    using System.Text.Json.Serialization;
    
    public class ShoppingCart
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        // Id must always be set explicitly to userId — Cosmos DB requires it to match the /userId partition key
        public string Id { get; set; } = string.Empty;

        [JsonProperty("userId")]
        [JsonPropertyName("userId")]
        // Cosmos partition key /userId is case-sensitive — must match exactly
        public string UserId { get; set; } = string.Empty;

        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}