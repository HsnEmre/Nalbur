using Nalbur.Domain.Entities;

namespace Nalbur.Domain.Interfaces;

public interface IProductService
{
    Task<List<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task DeleteAsync(int id);
    Task UpdateStockAsync(int productId, int quantity);
    Task<List<Product>> GetLowStockProductsAsync();
}
