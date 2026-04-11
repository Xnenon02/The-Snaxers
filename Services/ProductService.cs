using TheSnaxers.Models;

namespace TheSnaxers.Services;

public interface IProductService
{
    Task<List<Product>> GetAllProductsAsync();
}

public class ProductService : IProductService
{
    public async Task<List<Product>> GetAllProductsAsync()
    {
        // Vi simulerar att det tar en liten stund att hämta data (som ett riktigt API)
        await Task.Delay(100); 

        return new List<Product>
        {
            new Product { Id = 1, Name = "Mörk Sjösalt", Category = "Sweden", ImageUrl = "" },
            new Product { Id = 2, Name = "Belgisk Tryffel", Category = "Belgium", ImageUrl = "" },
            new Product { Id = 3, Name = "Ecuador 70%", Category = "Ecuador", ImageUrl = "" }
        };
    }
}