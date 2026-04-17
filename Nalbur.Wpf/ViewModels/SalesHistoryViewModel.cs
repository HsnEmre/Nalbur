using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

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

    public List<string> SaleTypeOptions { get; } = new() { "Hepsi", "Nakit", "Kart", "Taksit" };

    public SalesHistoryViewModel(ISaleService saleService, ICustomerService customerService)
    {
        _saleService = saleService;
        _customerService = customerService;
        
        SearchCommand = new AsyncRelayCommand(LoadSalesAsync);
        RefreshCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);

        Task.Run(async () => 
        {
            await LoadCustomersAsync();
            await LoadSalesAsync();
        });
    }

    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand RefreshCustomersCommand { get; }

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
