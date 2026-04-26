using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Wordprocessing;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class ReportsViewModel : ViewModelBase
{
    private readonly ISaleService _saleService;
    private readonly IOutgoingPaymentService _paymentService;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private ObservableCollection<ReportLine> _reportLines = new();

    [ObservableProperty]
    private decimal _totalIncome;

    [ObservableProperty]
    private decimal _totalExpense;

    [ObservableProperty]
    private decimal _netTotal;

    [ObservableProperty]
    private decimal _cashSalesTotal;

    [ObservableProperty]
    private decimal _cardSalesTotal;

    [ObservableProperty]
    private decimal _installmentSalesTotal;

    [ObservableProperty]
    private decimal _returnedSalesTotal;

    [ObservableProperty]
    private decimal _paidExpenseTotal;

    [ObservableProperty]
    private decimal _pendingExpenseTotal;

    [ObservableProperty]
    private decimal _overdueExpenseTotal;

    [ObservableProperty]
    private int _salesCount;

    [ObservableProperty]
    private int _expenseCount;

    public ReportsViewModel(ISaleService saleService, IOutgoingPaymentService paymentService)
    {
        _saleService = saleService;
        _paymentService = paymentService;

        LoadReportCommand = new AsyncRelayCommand(LoadReportAsync);

        ExportExcelCommand = new RelayCommand(() =>
            ExportHelper.ExportToExcel("Gelir Gider Raporu", ReportLines, ReportColumns()));

        ExportWordCommand = new RelayCommand(() =>
            ExportHelper.ExportToWord("Gelir Gider Raporu", ReportLines, ReportColumns()));

        ExportPdfCommand = new RelayCommand(() =>
            ExportHelper.ExportToPdf("Gelir Gider Raporu", ReportLines, ReportColumns()));

        LoadReportCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadReportCommand { get; }

    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportWordCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }

    private async Task LoadReportAsync()
    {
        var endDateInclusive = EndDate.Date.AddDays(1).AddSeconds(-1);

        var sales = await _saleService.GetFilteredSalesAsync(
            StartDate.Date,
            endDateInclusive,
            null,
            null);

        var payments = await _paymentService.GetFilteredPaymentsAsync(
            StartDate.Date,
            endDateInclusive,
            null,
            "Hepsi",
            null);

        var activeSales = sales
            .Where(x => !x.IsReturned)
            .ToList();

        var returnedSales = sales
            .Where(x => x.IsReturned)
            .ToList();

        TotalIncome = activeSales.Sum(x => x.TotalAmount);
        ReturnedSalesTotal = returnedSales.Sum(x => x.TotalAmount);

        CashSalesTotal = activeSales
            .Where(x => x.SaleType == SaleType.Cash)
            .Sum(x => x.TotalAmount);

        CardSalesTotal = activeSales
            .Where(x => x.SaleType == SaleType.Card)
            .Sum(x => x.TotalAmount);

        InstallmentSalesTotal = activeSales
            .Where(x => x.SaleType == SaleType.Installment)
            .Sum(x => x.TotalAmount);

        PaidExpenseTotal = payments
            .Where(x => x.IsPaid)
            .Sum(x => x.Amount);

        PendingExpenseTotal = payments
            .Where(x => !x.IsPaid && x.DueDate.Date >= DateTime.Today)
            .Sum(x => x.Amount);

        OverdueExpenseTotal = payments
            .Where(x => !x.IsPaid && x.DueDate.Date < DateTime.Today)
            .Sum(x => x.Amount);

        TotalExpense = PaidExpenseTotal;
        NetTotal = TotalIncome - TotalExpense;

        SalesCount = activeSales.Count;
        ExpenseCount = payments.Count;

        var lines = new List<ReportLine>();

        foreach (var sale in activeSales)
        {
            lines.Add(new ReportLine
            {
                Date = sale.SaleDate,
                Type = "Gelir",
                Source = "Satış",
                Description = GetSaleDescription(sale),
                Income = sale.TotalAmount,
                Expense = 0,
                Status = sale.SaleType.ToString()
            });
        }

        foreach (var sale in returnedSales)
        {
            lines.Add(new ReportLine
            {
                Date = sale.ReturnedAt ?? sale.SaleDate,
                Type = "İade",
                Source = "Satış İadesi",
                Description = GetSaleDescription(sale),
                Income = 0,
                Expense = 0,
                Status = "İade Edildi"
            });
        }

        foreach (var payment in payments)
        {
            lines.Add(new ReportLine
            {
                Date = payment.DueDate,
                Type = "Gider",
                Source = payment.Category,
                Description = payment.Title,
                Income = 0,
                Expense = payment.IsPaid ? payment.Amount : 0,
                Status = payment.IsPaid
                    ? "Ödenmiş"
                    : payment.DueDate.Date < DateTime.Today
                        ? "Gecikti"
                        : "Bekliyor"
            });
        }

        decimal runningBalance = 0;

        var orderedLines = lines
            .OrderBy(x => x.Date)
            .ThenBy(x => x.Type)
            .ToList();

        foreach (var line in orderedLines)
        {
            runningBalance += line.Income - line.Expense;
            line.Balance = runningBalance;
        }

        ReportLines = new ObservableCollection<ReportLine>(
            orderedLines.OrderByDescending(x => x.Date));
    }

    private static string GetSaleDescription(Sale sale)
    {
        var customer = $"{sale.Customer?.Name} {sale.Customer?.SurnameCompany}".Trim();

        if (string.IsNullOrWhiteSpace(customer))
            customer = "Perakende Müşteri";

        return $"Satış No: {sale.Id} - {customer}";
    }

    private static List<ExportColumn<ReportLine>> ReportColumns()
    {
        return new List<ExportColumn<ReportLine>>
        {
            new("Tarih", x => x.Date),
            new("Tip", x => x.Type),
            new("Kaynak", x => x.Source),
            new("Açıklama", x => x.Description),
            new("Gelir", x => x.Income),
            new("Gider", x => x.Expense),
            new("Bakiye", x => x.Balance),
            new("Durum", x => x.Status)
        };
    }
}

public class ReportLine
{
    public DateTime Date { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Income { get; set; }

    public decimal Expense { get; set; }

    public decimal Balance { get; set; }

    public string Status { get; set; } = string.Empty;
}