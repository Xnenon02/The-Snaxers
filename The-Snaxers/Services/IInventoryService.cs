namespace TheSnaxers.Services;

public interface IInventoryService
{
    Task<int> GetStockLevelAsync(string productId);
    Task<bool> DeductStockAsync(string productId, int quantity);
    Task<bool> IncreaseStockAsync(string productId, int quantity);
}