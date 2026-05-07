namespace TheSnaxers.Models;
public class CartItem
{
    public required string ProductId { get; set; }    
    public string ProductName { get; set; } =string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }

    public decimal TotalPrice => Quantity * Price;
}
