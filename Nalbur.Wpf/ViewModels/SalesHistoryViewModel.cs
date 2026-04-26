using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace Nalbur.Wpf.ViewModels;

public partial class SalesHistoryViewModel : ViewModelBase
{
    private readonly ISaleService _saleService;
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<Sale> _sales = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private Sale? _selectedSale;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private string? _saleTypeFilter = "Hepsi";

    public IAsyncRelayCommand ReturnSaleCommand { get; }

    public List<string> SaleTypeOptions { get; } = new() { "Hepsi", "Nakit", "Kart", "Taksit" };

    public SalesHistoryViewModel(ISaleService saleService, ICustomerService customerService)
    {
        _saleService = saleService;
        _customerService = customerService;
        
        SearchCommand = new AsyncRelayCommand(LoadSalesAsync);
        RefreshCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);
        DeleteSaleCommand = new AsyncRelayCommand(DeleteSaleAsync);

        ExportSelectedSaleInvoiceExcelCommand = new RelayCommand(ExportSelectedSaleInvoiceExcel);
        ExportSelectedSaleInvoiceWordCommand = new RelayCommand(ExportSelectedSaleInvoiceWord);
        ExportSelectedSaleInvoicePdfCommand = new RelayCommand(ExportSelectedSaleInvoicePdf);
        ReturnSaleCommand = new AsyncRelayCommand(ReturnSaleAsync);


        ExportExcelCommand = new RelayCommand(() =>
    ExportHelper.ExportToExcel("Satýþ Geçmiþi", Sales, SalesHistoryColumns()));

        ExportWordCommand = new RelayCommand(() =>
            ExportHelper.ExportToWord("Satýþ Geçmiþi", Sales, SalesHistoryColumns()));

       


        ExportPdfCommand = new RelayCommand(() =>
            ExportHelper.ExportToPdf("Satýþ Geçmiþi", Sales, SalesHistoryColumns()));
        Task.Run(async () => 
        {
            await LoadCustomersAsync();
            await LoadSalesAsync();
        });
    }
    public IAsyncRelayCommand DeleteSaleCommand { get; }

    public IRelayCommand ExportSelectedSaleInvoiceExcelCommand { get; }
    public IRelayCommand ExportSelectedSaleInvoiceWordCommand { get; }
    public IRelayCommand ExportSelectedSaleInvoicePdfCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand RefreshCustomersCommand { get; }
    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportWordCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }
    //private async Task LoadCustomersAsync()
    //{
    //    var customers = await _customerService.GetAllCustomersAsync();
    //    Customers = new ObservableCollection<Customer>(customers);
    //}
    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetAllAsync();
        Customers = new ObservableCollection<Customer>(customers);
    }
    private async Task DeleteSaleAsync()
    {
        if (SelectedSale == null)
            return;

        var result = System.Windows.MessageBox.Show(
            $"Seçili satýþ silinecek.\n\n" +
            $"Satýþ No: {SelectedSale.Id}\n" +
            $"Tutar: {SelectedSale.TotalAmount:C2}\n\n" +
            $"Bu iþlem satýþ kaydýný tamamen siler.\n" +
            $"Satýþ iade edilmemiþse ürünler stoða geri eklenir.\n\n" +
            $"Devam etmek istiyor musunuz?",
            "Satýþ Silme Onayý",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result != System.Windows.MessageBoxResult.Yes)
            return;

        try
        {
            await _saleService.DeleteSaleAsync(SelectedSale.Id, restoreStock: true);

            System.Windows.MessageBox.Show(
                "Satýþ baþarýyla silindi.",
                "Baþarýlý",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);

            SelectedSale = null;
            await LoadSalesAsync();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Satýþ silinirken hata oluþtu:\n{ex.Message}",
                "Hata",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    private void ExportSelectedSaleInvoiceExcel()
    {
        if (SelectedSale == null)
        {
            System.Windows.MessageBox.Show(
                "Lütfen fatura çýktýsý almak için bir satýþ seçin.",
                "Uyarý",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        InvoiceExportHelper.ExportSaleInvoiceToExcel(SelectedSale);
    }

    private void ExportSelectedSaleInvoiceWord()
    {
        if (SelectedSale == null)
        {
            System.Windows.MessageBox.Show(
                "Lütfen fatura çýktýsý almak için bir satýþ seçin.",
                "Uyarý",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        InvoiceExportHelper.ExportSaleInvoiceToWord(SelectedSale);
    }

    private void ExportSelectedSaleInvoicePdf()
    {
        if (SelectedSale == null)
        {
            System.Windows.MessageBox.Show(
                "Lütfen fatura çýktýsý almak için bir satýþ seçin.",
                "Uyarý",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        InvoiceExportHelper.ExportSaleInvoiceToPdf(SelectedSale);
    }

    private async Task ReturnSaleAsync()
    {
        if (SelectedSale == null)
            return;

        if (SelectedSale.IsReturned)
        {
            MessageBox.Show(
                "Bu satýþ zaten iade edilmiþ.",
                "Uyarý",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        var result = MessageBox.Show(
            $"Seçili satýþ iade edilecek.\n\n" +
            $"Satýþ No: {SelectedSale.Id}\n" +
            $"Tutar: {SelectedSale.TotalAmount:C2}\n\n" +
            $"Bu iþlem satýþtaki ürünleri stoða geri ekler.\n" +
            $"Taksitli satýþsa ödenmemiþ taksitleri iptal eder.\n\n" +
            $"Devam etmek istiyor musunuz?",
            "Satýþ Ýade Onayý",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            await _saleService.ReturnSaleAsync(
                SelectedSale.Id,
                "Satýþ geçmiþi ekranýndan iade edildi.");

            MessageBox.Show(
                "Satýþ baþarýyla iade edildi.",
                "Baþarýlý",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            SelectedSale = null;
            await LoadSalesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Ýade iþlemi sýrasýnda hata oluþtu:\n{ex.Message}",
                "Hata",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static List<ExportColumn<Sale>> SalesHistoryColumns()
    {
        return new List<ExportColumn<Sale>>
    {
        new("Satýþ Tarihi", x => x.SaleDate),
        new("Müþteri", x => $"{x.Customer?.Name} {x.Customer?.SurnameCompany}".Trim()),
        new("Telefon", x => x.Customer?.Phone),
        new("Satýþ Türü", x => x.SaleType),
        new("Toplam Tutar", x => x.TotalAmount),
        new("Ürün Sayýsý", x => x.SaleItems?.Count ?? 0),
        new("Ürünler", x => x.SaleItems == null
            ? ""
            : string.Join(", ", x.SaleItems.Select(i => $"{i.Product?.Name} x {i.Quantity}")))
    };
    }
    private async Task LoadSalesAsync()
    {
        SaleType? type = SaleTypeFilter switch
        {
            "Nakit" => SaleType.Cash,
            "Kart" => SaleType.Card,
            "Taksit" => SaleType.Installment,
            _ => null
        };

        var results = await _saleService.GetFilteredSalesAsync(StartDate, EndDate.AddDays(1).AddSeconds(-1), SelectedCustomer?.Id, type);
        Sales = new ObservableCollection<Sale>(results);
    }
}
