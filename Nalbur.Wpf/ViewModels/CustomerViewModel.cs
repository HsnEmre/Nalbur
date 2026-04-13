using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private Customer _newCustomer = new();

    public CustomerViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        LoadCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);
        SaveCustomerCommand = new AsyncRelayCommand(SaveCustomerAsync);
        
        LoadCustomersCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadCustomersCommand { get; }
    public IAsyncRelayCommand SaveCustomerCommand { get; }

    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetAllAsync();
        Customers = new ObservableCollection<Customer>(customers);
    }

    private async Task SaveCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCustomer.Name)) return;

        await _customerService.AddAsync(NewCustomer);
        NewCustomer = new Customer();
        await LoadCustomersAsync();
    }
}
