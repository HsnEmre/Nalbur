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

    [ObservableProperty]
    private bool _isEditMode;

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value != null)
        {
            // Create a copy to edit
            NewCustomer = new Customer
            {
                Id = value.Id,
                Name = value.Name,
                SurnameCompany = value.SurnameCompany,
                Phone = value.Phone,
                Email = value.Email,
                Address = value.Address
            };
            IsEditMode = true;
        }
        else
        {
            ClearForm();
        }
    }

    public CustomerViewModel(ICustomerService customerService)
    {
        _customerService = customerService;
        LoadCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);
        SaveCustomerCommand = new AsyncRelayCommand(SaveCustomerAsync);
        DeleteCustomerCommand = new AsyncRelayCommand(DeleteCustomerAsync);
        ClearFormCommand = new RelayCommand(ClearForm);
        
        LoadCustomersCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadCustomersCommand { get; }
    public IAsyncRelayCommand SaveCustomerCommand { get; }
    public IAsyncRelayCommand DeleteCustomerCommand { get; }
    public IRelayCommand ClearFormCommand { get; }

    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetAllAsync();
        Customers = new ObservableCollection<Customer>(customers);
    }

    private async Task SaveCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCustomer.Name)) return;

        if (IsEditMode)
        {
            await _customerService.UpdateAsync(NewCustomer);
        }
        else
        {
            await _customerService.AddAsync(NewCustomer);
        }
        
        ClearForm();
        await LoadCustomersAsync();
    }

    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        // In a real app, I'd check for existing sales/installments via a service call.
        // For now, I'll implement a safety warning.
        var msg = $"{SelectedCustomer.Name} {SelectedCustomer.SurnameCompany} silinecek.\n\n" +
                  "UYARI: Bu müşteriye ait satış veya taksit verisi varsa silme işlemi başarısız olabilir.";

        if (System.Windows.MessageBox.Show(msg, "Onay", 
            System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
        {
            try 
            {
                await _customerService.DeleteAsync(SelectedCustomer.Id);
                ClearForm();
                await LoadCustomersAsync();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Silme hatası: {ex.Message}\n\nBu müşteri muhtemelen sistemde işlem görmüş (Satış/Taksit).", 
                    "Hata", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void ClearForm()
    {
        NewCustomer = new Customer();
        IsEditMode = false;
        SelectedCustomer = null;
    }
}
