using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;

namespace Nalbur.Infrastructure.Services;

public class InstallmentService : IInstallmentService
{
    private readonly NalburDbContext _context;

    public InstallmentService(NalburDbContext context)
    {
        _context = context;
    }

    public async Task<List<Installment>> GetOverdueInstallmentsAsync()
    {
        return await _context.Installments
            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
                    .ThenInclude(s => s.Customer)
            .Where(i => i.Status != InstallmentStatus.Paid && i.DueDate < DateTime.Today)
            .ToListAsync();
    }

    public async Task<List<Installment>> GetUpcomingInstallmentsAsync(int days)
    {
        var targetDate = DateTime.Today.AddDays(days);
        return await _context.Installments
            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
                    .ThenInclude(s => s.Customer)
            .Where(i => i.Status != InstallmentStatus.Paid && i.DueDate >= DateTime.Today && i.DueDate <= targetDate)
            .ToListAsync();
    }

    public async Task<List<Installment>> GetInstallmentsByCustomerAsync(int customerId)
    {
        return await _context.Installments
            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
            .Where(i => i.InstallmentPlan.Sale.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task MarkAsPaidAsync(int installmentId)
    {
        var installment = await _context.Installments.FindAsync(installmentId);
        if (installment != null)
        {
            installment.Status = InstallmentStatus.Paid;
            installment.PaymentDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}

public class ReminderService : IReminderService
{
    private readonly NalburDbContext _context;

    public ReminderService(NalburDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetOverdueCountAsync()
    {
        return await _context.Installments
            .CountAsync(i => i.Status != InstallmentStatus.Paid && i.DueDate < DateTime.Today);
    }

    public async Task<int> GetLowStockCountAsync()
    {
        return await _context.Products
            .CountAsync(p => p.CurrentStock <= p.MinimumStock);
    }

    public async Task<List<Installment>> GetTodayDueInstallmentsAsync()
    {
        return await _context.Installments
            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
                    .ThenInclude(s => s.Customer)
            .Where(i => i.Status != InstallmentStatus.Paid && i.DueDate.Date == DateTime.Today)
            .ToListAsync();
    }
}
