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
            .OrderByDescending(s => s.SaleDate)
            .ToListAsync();
    }

    public async Task<List<Sale>> GetFilteredSalesAsync(DateTime? startDate, DateTime? endDate, int? customerId, SaleType? saleType)
    {
        var query = _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.InstallmentPlan)
                .ThenInclude(ip => ip != null ? ip.Installments : null)
            .AsQueryable();

        if (startDate.HasValue)
            query = query.Where(s => s.SaleDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.SaleDate <= endDate.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (saleType.HasValue)
            query = query.Where(s => s.SaleType == saleType.Value);

        return await query.OrderByDescending(s => s.SaleDate).ToListAsync();
    }
    public async Task DeleteSaleAsync(int saleId, bool restoreStock = true)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.InstallmentPlan)
                .ThenInclude(ip => ip.Installments)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null)
            throw new Exception("Satýþ bulunamadý.");

        // Satýþ daha önce iade edilmemiþse stoklarý geri ekle.
        // Eðer IsReturned true ise zaten stok geri dönmüþtür, tekrar eklemiyoruz.
        if (restoreStock && !sale.IsReturned)
        {
            foreach (var item in sale.SaleItems)
            {
                var product = item.Product;

                if (product == null)
                {
                    product = await _context.Products
                        .FirstOrDefaultAsync(p => p.Id == item.ProductId);
                }

                if (product != null)
                {
                    product.CurrentStock += item.Quantity;
                }
            }
        }

        if (sale.InstallmentPlan != null)
        {
            if (sale.InstallmentPlan.Installments != null && sale.InstallmentPlan.Installments.Any())
            {
                _context.Installments.RemoveRange(sale.InstallmentPlan.Installments);
            }

            _context.InstallmentPlans.Remove(sale.InstallmentPlan);
        }

        if (sale.SaleItems != null && sale.SaleItems.Any())
        {
            _context.SaleItems.RemoveRange(sale.SaleItems);
        }

        _context.Sales.Remove(sale);

        await _context.SaveChangesAsync();
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
    public async Task ReturnSaleAsync(int saleId, string? note = null)
    {
        var sale = await _context.Sales
            .Include(s => s.SaleItems)
                .ThenInclude(si => si.Product)
            .Include(s => s.InstallmentPlan)
                .ThenInclude(ip => ip.Installments)
            .FirstOrDefaultAsync(s => s.Id == saleId);

        if (sale == null)
            throw new Exception("Satýþ bulunamadý.");

        if (sale.IsReturned)
            throw new Exception("Bu satýþ zaten iade edilmiþ.");

        if (sale.SaleItems == null || !sale.SaleItems.Any())
            throw new Exception("Satýþa ait ürün bulunamadý.");

        foreach (var item in sale.SaleItems)
        {
            var product = item.Product;

            if (product == null)
            {
                product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == item.ProductId);
            }

            if (product == null)
                throw new Exception($"Ürün bulunamadý. ProductId: {item.ProductId}");

            // Satýlan ürün stoða geri eklenir
            product.CurrentStock += item.Quantity;
        }

        sale.IsReturned = true;
        sale.ReturnedAt = DateTime.Now;
        sale.ReturnNote = note;

        // Taksitli satýþsa ödenmemiþ taksitleri iptal et
        if (sale.InstallmentPlan != null)
        {
            foreach (var installment in sale.InstallmentPlan.Installments)
            {
                if (installment.Status != InstallmentStatus.Paid)
                {
                    installment.Status = InstallmentStatus.Cancelled;
                }
            }
        }

        await _context.SaveChangesAsync();
    }


    public async Task<Sale> ProcessSaleAsync(Sale sale, InstallmentPlan? plan)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var newSale = new Sale
            {
                SaleDate = sale.SaleDate == default ? DateTime.Now : sale.SaleDate,
                CustomerId = sale.CustomerId,
                TotalAmount = sale.TotalAmount,
                SaleType = sale.SaleType,
                SaleItems = sale.SaleItems.Select(x => new SaleItem
                {
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    UnitPrice = x.UnitPrice,
                    TotalPrice = x.TotalPrice
                }).ToList()
            };

            await _context.Sales.AddAsync(newSale);

            foreach (var item in newSale.SaleItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Ürün bulunamadý. ProductId={item.ProductId}");

                if (product.CurrentStock < item.Quantity)
                    throw new InvalidOperationException($"{product.Name} için yeterli stok yok.");

                product.CurrentStock -= item.Quantity;
            }

            if (newSale.SaleType == SaleType.Installment && plan != null)
            {
                var newPlan = new InstallmentPlan
                {
                    Sale = newSale,
                    TotalAmount = plan.TotalAmount,
                    DownPayment = plan.DownPayment,
                    InstallmentCount = plan.InstallmentCount,
                    Installments = plan.Installments.Select(i => new Installment
                    {
                        Amount = i.Amount,
                        DueDate = i.DueDate,
                        Status = i.Status,
                        PaidAmount = i.PaidAmount
                    }).ToList()
                };

                await _context.InstallmentPlans.AddAsync(newPlan);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return newSale;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    //public async Task<Sale> ProcessSaleAsync(Sale sale, InstallmentPlan? plan)
    //{
    //    using var transaction = await _context.Database.BeginTransactionAsync();
    //    try
    //    {
    //        // 1. Add Sale
    //        await _context.Sales.AddAsync(sale);

    //        // 2. Update Stocks
    //        foreach (var item in sale.SaleItems)
    //        {
    //            var product = await _context.Products.FindAsync(item.ProductId);
    //            if (product != null)
    //            {
    //                product.CurrentStock -= item.Quantity;
    //            }
    //        }

    //        // 3. Handle Installments if applicable
    //        if (sale.SaleType == SaleType.Installment && plan != null)
    //        {
    //            plan.Sale = sale;
    //            await _context.InstallmentPlans.AddAsync(plan);
    //        }

    //        await _context.SaveChangesAsync();
    //        await transaction.CommitAsync();

    //        return sale;
    //    }
    //    catch
    //    {
    //        await transaction.RollbackAsync();
    //        throw;
    //    }
    //}
}
