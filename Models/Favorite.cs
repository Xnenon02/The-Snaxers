namespace TheSnaxers.Models;

public class Favorite
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public DateTime SavedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Product? Product { get; set; }
}