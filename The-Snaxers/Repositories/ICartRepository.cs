using TheSnaxers.Models;

public interface ICartRepository
{
    // Hämta hela "lådan" (ShoppingCart-objektet) för en specifik person
    Task<ShoppingCart> GetCartByUserIdAsync(string userId);
    
    // Spara/Uppdatera hela "lådan" i ett svep
    Task SaveCartAsync(ShoppingCart cart);

    // Om ni vill ha en specifik metod för att tömma
    Task ClearCartAsync(string userId);
}