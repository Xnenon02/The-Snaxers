using TheSnaxers.Models;

namespace TheSnaxers.Repositories;

public interface ICartRepository
{
    // Hämtar vagnen för en specifik användare
    // Om ingen vagn finns, returnerar den en ny, tom vagn
    Task<ShoppingCart> GetCartByUserIdAsync(string userId);

    // Sparar eller uppdaterar hela vagnen i Cosmos
    Task UpsertCartAsync(ShoppingCart cart);

    // Tömmer vagnen (t.ex. efter ett köp)
    Task ClearCartAsync(string userId);
}