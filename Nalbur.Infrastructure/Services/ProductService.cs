using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;

namespace Nalbur.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly NalburDbContext _context;

    public ProductService(NalburDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllAsync() => await _context.Products.AsNoTracking().ToListAsync();

    public async Task<Product?> GetByIdAsync(int id) => await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task AddAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Product product)
    {
        var existingProduct = await _context.Products.FindAsync(product.Id);
        if (existingProduct != null)
        {
            existingProduct.Code = product.Code;
            existingProduct.Barcode = product.Barcode;
            existingProduct.Name = product.Name;
            existingProduct.Category = product.Category;
            existingProduct.CostPrice = product.CostPrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.CurrentStock = product.CurrentStock;
            existingProduct.MinimumStock = product.MinimumStock;
            existingProduct.IsActive = product.IsActive;
            
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateStockAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            product.CurrentStock += quantity;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Product>> GetLowStockProductsAsync()
    {
        return await _context.Products
            .Where(p => p.CurrentStock <= p.MinimumStock)
            .ToListAsync();
    }
}
