using Nalbur.Domain.Entities;

namespace Nalbur.Domain.Interfaces;

public interface ISaleService
{
    Task<List<Sale>> GetAllSalesAsync();
    Task<Sale?> GetSaleByIdAsync(int id);
    Task<Sale> ProcessSaleAsync(Sale sale, InstallmentPlan? plan);
}

public interface IInstallmentService
{
    Task<List<Installment>> GetOverdueInstallmentsAsync();
    Task<List<Installment>> GetUpcomingInstallmentsAsync(int days);
    Task<List<Installment>> GetActiveInstallmentsAsync();
    Task<List<Installment>> GetInstallmentsByCustomerAsync(int customerId);
    Task ProcessPaymentAsync(int installmentId, decimal amount);
}

public interface IReminderService
{
    Task<int> GetOverdueCountAsync();
    Task<int> GetLowStockCountAsync();
    Task<List<Installment>> GetTodayDueInstallmentsAsync();
}
