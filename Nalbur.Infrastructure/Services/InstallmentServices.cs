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

    public async Task<List<Installment>> GetActiveInstallmentsAsync()
    {
        // En az 1 tane ödenmemiþ taksiti olan planlarý bul
        var activePlanIds = await _context.Installments
            .Where(i => i.Status != InstallmentStatus.Paid)
            .Select(i => i.InstallmentPlanId)
            .Distinct()
            .ToListAsync();

        if (!activePlanIds.Any())
            return new List<Installment>();

        // Bu aktif planlara ait tüm taksitleri getir
        // Paid olanlarý da getiriyoruz ki geçmiþ ödeme/taksit durumu tabloda görünsün
        return await _context.Installments
            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
                    .ThenInclude(s => s.Customer)

            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
                    .ThenInclude(s => s.SaleItems)
                        .ThenInclude(si => si.Product)

            .Where(i => activePlanIds.Contains(i.InstallmentPlanId))
            .OrderBy(i => i.InstallmentPlanId)
            .ThenBy(i => i.DueDate)
            .ToListAsync();
    }

    public async Task<List<Installment>> GetInstallmentsByCustomerAsync(int customerId)
    {
        return await _context.Installments
            .Include(i => i.InstallmentPlan)
                .ThenInclude(ip => ip.Sale)
                    .ThenInclude(s => s.Customer)
            .Include(i => i.InstallmentPlan.Sale.SaleItems)
                .ThenInclude(si => si.Product)
            .Where(i => i.InstallmentPlan.Sale.CustomerId == customerId)
            .ToListAsync();
    }

    public async Task ProcessPaymentAsync(int installmentId, decimal amount)
    {
        if (amount <= 0) return;

        var installment = await _context.Installments
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == installmentId);

        if (installment != null)
        {
            // Do not allow overpayment to corrupt state - cap it at remaining amount
            var remaining = installment.Amount - installment.PaidAmount;
            if (amount > remaining)
            {
                amount = remaining;
            }

            if (amount <= 0) return;

            // 1. Create Payment Record
            var payment = new Payment
            {
                InstallmentId = installmentId,
                Amount = amount,
                PaymentDate = DateTime.Now
            };
            await _context.Payments.AddAsync(payment);

            // 2. Update Installment
            installment.PaidAmount += amount;
            installment.PaymentDate = payment.PaymentDate;

            // Only mark as Paid when fully completed
            if (installment.PaidAmount >= installment.Amount)
            {
                installment.Status = InstallmentStatus.Paid;
            }
            else
            {
                // Ensure it's not marked as Paid if only partially paid
                // (It could be Overdue or Pending depending on date, but definitely not Paid)
                if (installment.Status == InstallmentStatus.Paid)
                {
                    installment.Status = installment.DueDate < DateTime.Today 
                        ? InstallmentStatus.Overdue 
                        : InstallmentStatus.Pending;
                }
            }

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

    public async Task<int> GetActiveInstallmentCountAsync()
    {
        return await _context.Installments
            .Where(i => i.Status != InstallmentStatus.Paid)
            .Select(i => i.InstallmentPlanId)
            .Distinct()
            .CountAsync();
    }
}
