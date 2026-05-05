namespace TheSnaxers.Services;

using TheSnaxers.Models;
using TheSnaxers.Repositories;

public class InventoryService : IInventoryService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IProductRepository productRepository, ILogger<InventoryService> inventoryService)
    {
        _productRepository = productRepository;
        _logger = inventoryService;
    }

    public async Task<int> GetStockLevelAsync(string productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new KeyNotFoundException($"Produkten med id {productId} hittades inte.");

        return product.StockLevel;
    }

    public async Task<bool> DeductStockAsync(string productId, int quantity)
    {
        if (quantity <= 0) return false;

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null || product.StockLevel < quantity) return false;

        // Hämta originalkategori innan vi ändrar i objektet
        string originalCategory = product.Category;

        product.StockLevel -= quantity;
        await _productRepository.UpdateAsync(product, originalCategory);
        
        _logger.LogInformation("Lagersaldo minskat för produkt {ProductId} med {Quantity} enheter.", productId, quantity);
        return true;
    }

    public async Task<bool> IncreaseStockAsync(string productId, int quantity)
    {
        if (quantity <= 0) return false;

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null) return false;

        // Hämta originalkategori innan vi ändrar i objektet
        string originalCategory = product.Category;

        product.StockLevel += quantity;
        await _productRepository.UpdateAsync(product, originalCategory);

        _logger.LogInformation("Lagersaldo ökat för produkt {ProductId} med {Quantity} enheter.", productId, quantity);
        return true;
    }
}