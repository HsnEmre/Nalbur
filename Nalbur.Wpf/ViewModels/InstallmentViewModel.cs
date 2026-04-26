using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class InstallmentViewModel : ViewModelBase
{
    private readonly IInstallmentService _installmentService;
    private List<Installment> _allInstallments = new();

    [ObservableProperty]
    private ObservableCollection<Installment> _installments = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterInstallments();
            }
        }
    }

    [ObservableProperty]
    private Installment? _selectedInstallment;

    [ObservableProperty]
    private decimal _paymentAmount;

    [ObservableProperty]
    private ObservableCollection<SaleItem> _selectedSaleItems = new();

    [ObservableProperty]
    private Sale? _selectedSale;

    partial void OnSelectedInstallmentChanged(Installment? value)
    {
        if (value != null)
        {
            PaymentAmount = value.RemainingAmount;
            SelectedSale = value.InstallmentPlan?.Sale;
            SelectedSaleItems = new ObservableCollection<SaleItem>(
                value.InstallmentPlan?.Sale?.SaleItems ?? new List<SaleItem>());
        }
        else
        {
            SelectedSale = null;
            SelectedSaleItems.Clear();
        }
    }

    public InstallmentViewModel(IInstallmentService installmentService)
    {
        _installmentService = installmentService;

        RefreshCommand = new AsyncRelayCommand(LoadInstallmentsAsync);
        PayInstallmentCommand = new AsyncRelayCommand(PayInstallmentAsync);
        ExportExcelCommand = new RelayCommand(() =>
    ExportHelper.ExportToExcel("Taksit Takip Listesi", Installments, InstallmentColumns()));

        ExportWordCommand = new RelayCommand(() =>
            ExportHelper.ExportToWord("Taksit Takip Listesi", Installments, InstallmentColumns()));

        ExportPdfCommand = new RelayCommand(() =>
            ExportHelper.ExportToPdf("Taksit Takip Listesi", Installments, InstallmentColumns()));
        RefreshCommand.Execute(null);
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand PayInstallmentCommand { get; }
    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportWordCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }
    private async Task LoadInstallmentsAsync()
    {
        var allActive = await _installmentService.GetActiveInstallmentsAsync();

        _allInstallments = allActive
            .OrderBy(x => x.DueDate)
            .ToList();

        FilterInstallments();
    }
    private static List<ExportColumn<Installment>> InstallmentColumns()
    {
        return new List<ExportColumn<Installment>>
    {
        new("Müþteri", x => $"{x.InstallmentPlan?.Sale?.Customer?.Name} {x.InstallmentPlan?.Sale?.Customer?.SurnameCompany}".Trim()),
        new("Telefon", x => x.InstallmentPlan?.Sale?.Customer?.Phone),
        new("Taksit Tutarý", x => x.Amount),
        new("Ödenen", x => x.PaidAmount),
        new("Kalan", x => x.RemainingAmount),
        new("Vade Tarihi", x => x.DueDate),
        new("Son Ödeme Tarihi", x => x.PaymentDate),
        new("Taksit Durumu", x => x.Status),
        new("Satýþ Toplamý", x => x.InstallmentPlan?.Sale?.TotalAmount),
        new("Peþinat", x => x.InstallmentPlan?.DownPayment)
    };
    }
    private void FilterInstallments()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Installments = new ObservableCollection<Installment>(_allInstallments);
            return;
        }

        var search = SearchText.Trim();

        var filtered = _allInstallments
            .Where(i =>
            {
                var customer = i.InstallmentPlan?.Sale?.Customer;

                if (customer == null)
                    return false;

                return
                    (!string.IsNullOrWhiteSpace(customer.Name) &&
                     customer.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||

                    (!string.IsNullOrWhiteSpace(customer.SurnameCompany) &&
                     customer.SurnameCompany.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||

                    (!string.IsNullOrWhiteSpace(customer.Phone) &&
                     customer.Phone.Contains(search, StringComparison.CurrentCultureIgnoreCase));
            })
            .ToList();

        Installments = new ObservableCollection<Installment>(filtered);
    }

    private async Task PayInstallmentAsync()
    {
        if (SelectedInstallment == null || PaymentAmount <= 0)
            return;

        await _installmentService.ProcessPaymentAsync(SelectedInstallment.Id, PaymentAmount);

        SelectedInstallment = null;
        PaymentAmount = 0;

        await LoadInstallmentsAsync();
    }
}