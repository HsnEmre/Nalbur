using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;

namespace Nalbur.Infrastructure.Services;

public class SaleService : ISaleService
{
    private readonly NalburDbContext _context;

    public SaleService(NalburDbContext context)
    {
        _context = context;
    }

    public async Task<List<Sale>> GetAllSalesAsync()
    {
        return await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.InstallmentPlan)
            .ToListAsync();
    }

    public async Task<Sale?> GetSaleByIdAsync(int id)
    {
        return await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.InstallmentPlan)
                .ThenInclude(ip => ip.Installments)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sale> ProcessSaleAsync(Sale sale, InstallmentPlan? plan)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Add Sale
            await _context.Sales.AddAsync(sale);
            
            // 2. Update Stocks
            foreach (var item in sale.SaleItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.CurrentStock -= item.Quantity;
                }
            }

            // 3. Handle Installments if applicable
            if (sale.SaleType == SaleType.Installment && plan != null)
            {
                plan.Sale = sale;
                await _context.InstallmentPlans.AddAsync(plan);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return sale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
