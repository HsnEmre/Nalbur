using CommunityToolkit.Mvvm.ComponentModel;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IReminderService _reminderService;
    private readonly IOutgoingPaymentService _paymentService;
    private readonly IProductService _productService;
    private readonly IInstallmentService _installmentService;

    [ObservableProperty]
    private int _overdueCount;

    [ObservableProperty]
    private int _lowStockCount;

    [ObservableProperty]
    private int _upcomingPaymentsCount;

    [ObservableProperty]
    private int _overduePaymentsCount;

    public ObservableCollection<Product> LowStockProducts { get; } = new();
    public ObservableCollection<Installment> TodayDueInstallments { get; } = new();
    public ObservableCollection<Installment> UpcomingInstallments { get; } = new();
    public ObservableCollection<Installment> OverdueInstallments { get; } = new();
    public ObservableCollection<OutgoingPayment> UpcomingOutgoingPayments { get; } = new();
    public ObservableCollection<OutgoingPayment> OverdueOutgoingPayments { get; } = new();

    public DashboardViewModel(
        IReminderService reminderService,
        IOutgoingPaymentService paymentService,
        IProductService productService,
        IInstallmentService installmentService)
    {
        _reminderService = reminderService;
        _paymentService = paymentService;
        _productService = productService;
        _installmentService = installmentService;

        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        OverdueCount = await _reminderService.GetOverdueCountAsync();
        LowStockCount = await _reminderService.GetLowStockCountAsync();
        UpcomingPaymentsCount = await _paymentService.GetUpcomingCountAsync(7);
        OverduePaymentsCount = await _paymentService.GetOverdueCountAsync();

        // Düþük stok ürünler
        var products = await _productService.GetAllAsync();
        LowStockProducts.Clear();
        foreach (var item in products
                     .Where(x => x.IsActive && x.CurrentStock <= x.MinimumStock)
                     .OrderBy(x => x.CurrentStock))
        {
            LowStockProducts.Add(item);
        }

        // Bugün vadeli taksitler
        var todayDue = await _reminderService.GetTodayDueInstallmentsAsync();
        TodayDueInstallments.Clear();
        foreach (var item in todayDue.OrderBy(x => x.DueDate))
        {
            TodayDueInstallments.Add(item);
        }

        // Aktif taksitlerden yaklaþan / gecikmiþ ayýr
        var activeInstallments = await _installmentService.GetActiveInstallmentsAsync();

        var today = DateTime.Today;
        var next7 = today.AddDays(7);

        UpcomingInstallments.Clear();
        foreach (var item in activeInstallments
                     .Where(x => x.DueDate.Date > today && x.DueDate.Date <= next7)
                     .OrderBy(x => x.DueDate))
        {
            UpcomingInstallments.Add(item);
        }

        OverdueInstallments.Clear();
        foreach (var item in activeInstallments
                     .Where(x => x.DueDate.Date < today)
                     .OrderBy(x => x.DueDate))
        {
            OverdueInstallments.Add(item);
        }

        // Firma ödemeleri
        var outgoing = await _paymentService.GetFilteredPaymentsAsync(
            DateTime.Today.AddMonths(-3),
            DateTime.Today.AddMonths(3),
            null,
            "Hepsi",
            null);

        UpcomingOutgoingPayments.Clear();
        foreach (var item in outgoing
                     .Where(x => !x.IsPaid && x.DueDate.Date >= today && x.DueDate.Date <= next7)
                     .OrderBy(x => x.DueDate))
        {
            UpcomingOutgoingPayments.Add(item);
        }

        OverdueOutgoingPayments.Clear();
        foreach (var item in outgoing
                     .Where(x => !x.IsPaid && x.DueDate.Date < today)
                     .OrderBy(x => x.DueDate))
        {
            OverdueOutgoingPayments.Add(item);
        }
    }
}