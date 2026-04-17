using Microsoft.EntityFrameworkCore;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using Nalbur.Infrastructure.Data;

namespace Nalbur.Infrastructure.Services;

public class OutgoingPaymentService : IOutgoingPaymentService
{
    private readonly NalburDbContext _context;

    public OutgoingPaymentService(NalburDbContext context)
    {
        _context = context;
    }

    public async Task<List<OutgoingPayment>> GetFilteredPaymentsAsync(DateTime? startDate, DateTime? endDate, OutgoingPaymentStatus? status, string? category, string? searchText)
    {
        var query = _context.OutgoingPayments.AsQueryable();

        if (startDate.HasValue)
            query = query.Where(p => p.DueDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(p => p.DueDate <= endDate.Value);

        if (category != null && category != "Hepsi")
            query = query.Where(p => p.Category == category);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(p => p.Title.Contains(searchText) || (p.Description != null && p.Description.Contains(searchText)));

        var payments = await query.OrderByDescending(p => p.DueDate).ToListAsync();

        if (status.HasValue)
        {
            payments = payments.Where(p => p.Status == status.Value).ToList();
        }

        return payments;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _context.OutgoingPayments
            .Select(p => p.Category)
            .Distinct()
            .ToListAsync();
    }

    public async Task<OutgoingPayment> AddPaymentAsync(OutgoingPayment payment)
    {
        _context.OutgoingPayments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task MarkAsPaidAsync(int paymentId)
    {
        var payment = await _context.OutgoingPayments.FindAsync(paymentId);
        if (payment != null)
        {
            payment.IsPaid = true;
            payment.PaymentDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUpcomingCountAsync(int days)
    {
        var targetDate = DateTime.Today.AddDays(days);
        return await _context.OutgoingPayments
            .CountAsync(p => !p.IsPaid && p.DueDate >= DateTime.Today && p.DueDate <= targetDate);
    }

    public async Task<int> GetOverdueCountAsync()
    {
        return await _context.OutgoingPayments
            .CountAsync(p => !p.IsPaid && p.DueDate < DateTime.Today);
    }
}
