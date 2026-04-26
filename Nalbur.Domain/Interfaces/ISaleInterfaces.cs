using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;

namespace Nalbur.Domain.Interfaces;

public interface ISaleService
{
    Task<List<Sale>> GetAllSalesAsync();
    Task<List<Sale>> GetFilteredSalesAsync(DateTime? startDate, DateTime? endDate, int? customerId, SaleType? saleType);
    Task<Sale?> GetSaleByIdAsync(int id);
    Task<Sale> ProcessSaleAsync(Sale sale, InstallmentPlan? plan);
    Task ReturnSaleAsync(int saleId, string? note = null);

    Task DeleteSaleAsync(int saleId, bool restoreStock = true);
}

public interface IInstallmentService
{
    Task<List<Installment>> GetOverdueInstallmentsAsync();
    Task<List<Installment>> GetUpcomingInstallmentsAsync(int days);
    Task<List<Installment>> GetActiveInstallmentsAsync();
    Task<List<Installment>> GetInstallmentsByCustomerAsync(int customerId);
    Task ProcessPaymentAsync(int installmentId, decimal amount);
}

public interface IOutgoingPaymentService
{
    Task<List<OutgoingPayment>> GetFilteredPaymentsAsync(DateTime? startDate, DateTime? endDate, OutgoingPaymentStatus? status, string? category, string? searchText);
    Task<List<string>> GetCategoriesAsync();
    Task<OutgoingPayment> AddPaymentAsync(OutgoingPayment payment);
    Task MarkAsPaidAsync(int paymentId);
    Task<int> GetUpcomingCountAsync(int days);
    Task<int> GetOverdueCountAsync();
}

public interface IReminderService
{
    Task<int> GetOverdueCountAsync();
    Task<int> GetLowStockCountAsync();
    Task<List<Installment>> GetTodayDueInstallmentsAsync();
    Task<int> GetActiveInstallmentCountAsync();
}
