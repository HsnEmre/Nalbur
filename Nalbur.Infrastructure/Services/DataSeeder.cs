using Nalbur.Domain.Entities;
using Nalbur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Nalbur.Infrastructure.Services;

public class DataSeeder
{
    private readonly NalburDbContext _context;

    public DataSeeder(NalburDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // 1. Ensure Database is created
        await _context.Database.MigrateAsync();

        // 2. Seed User if none exists
        if (!await _context.Users.AnyAsync())
        {
            await _context.Users.AddAsync(new User { Username = "admin", PasswordHash = "admin", Role = "Admin" });
        }

        // 3. Seed Products if none exist
        if (!await _context.Products.AnyAsync())
        {
            await _context.Products.AddRangeAsync(new List<Product>
            {
                new Product { Code = "P001", Name = "Çekiç", Category = "El Aletleri", CostPrice = 50, SalePrice = 120, CurrentStock = 20, MinimumStock = 5 },
                new Product { Code = "P002", Name = "Tornavida Seti", Category = "El Aletleri", CostPrice = 80, SalePrice = 180, CurrentStock = 15, MinimumStock = 3 },
                new Product { Code = "P003", Name = "Boya Fırçası 4\"", Category = "Boya", CostPrice = 10, SalePrice = 25, CurrentStock = 50, MinimumStock = 10 },
                new Product { Code = "P004", Name = "Pense", Category = "El Aletleri", CostPrice = 40, SalePrice = 95, CurrentStock = 2, MinimumStock = 5 }, // Low stock
            });
        }

        // 4. Seed Customers if none exist
        if (!await _context.Customers.AnyAsync())
        {
            await _context.Customers.AddRangeAsync(new List<Customer>
            {
                new Customer { Name = "Ahmet", SurnameCompany = "Yılmaz", Phone = "05551112233", Email = "ahmet@mail.com" },
                new Customer { Name = "Mehmet", SurnameCompany = "Kaya İnşaat", Phone = "05554445566", Email = "mehmet@kaya.com", Address = "İstanbul" },
            });
        }

        await _context.SaveChangesAsync();
    }
}
