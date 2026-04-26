using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class CustomerViewModel : ViewModelBase
{
    private readonly ICustomerService _customerService;
    private List<Customer> _allCustomers = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterCustomers();
            }
        }
    }

   

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
        ExportExcelCommand = new RelayCommand(() =>
    ExportHelper.ExportToExcel("Müşteri Listesi", Customers, CustomerColumns()));

        ExportWordCommand = new RelayCommand(() =>
            ExportHelper.ExportToWord("Müşteri Listesi", Customers, CustomerColumns()));

        ExportPdfCommand = new RelayCommand(() =>
            ExportHelper.ExportToPdf("Müşteri Listesi", Customers, CustomerColumns()));
        LoadCustomersCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadCustomersCommand { get; }
    public IAsyncRelayCommand SaveCustomerCommand { get; }
    public IAsyncRelayCommand DeleteCustomerCommand { get; }
    public IRelayCommand ClearFormCommand { get; }
    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportWordCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }
    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetAllAsync();

        _allCustomers = customers
            .OrderBy(x => x.Name)
            .ThenBy(x => x.SurnameCompany)
            .ToList();

        FilterCustomers();
    }

    private void FilterCustomers()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Customers = new ObservableCollection<Customer>(_allCustomers);
            return;
        }

        var search = SearchText.Trim();

        var filteredCustomers = _allCustomers
            .Where(c =>
                (!string.IsNullOrWhiteSpace(c.Name) &&
                 c.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||

                (!string.IsNullOrWhiteSpace(c.SurnameCompany) &&
                 c.SurnameCompany.Contains(search, StringComparison.CurrentCultureIgnoreCase)) ||

                (!string.IsNullOrWhiteSpace(c.Phone) &&
                 c.Phone.Contains(search, StringComparison.CurrentCultureIgnoreCase)))
            .ToList();

        Customers = new ObservableCollection<Customer>(filteredCustomers);
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

        var msg = $"{SelectedCustomer.Name} {SelectedCustomer.SurnameCompany} silinecek.\n\n" +
                  "UYARI: Bu müşteriye ait satış veya taksit verisi varsa silme işlemi başarısız olabilir.";

        if (System.Windows.MessageBox.Show(
                msg,
                "Onay",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _customerService.DeleteAsync(SelectedCustomer.Id);
                ClearForm();
                await LoadCustomersAsync();
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Silme hatası: {ex.Message}\n\nBu müşteri muhtemelen sistemde işlem görmüş (Satış/Taksit).",
                    "Hata",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
    }

    private void ClearForm()
    {
        NewCustomer = new Customer();
        IsEditMode = false;
        SelectedCustomer = null;
    }
    private static List<ExportColumn<Customer>> CustomerColumns()
    {
        return new List<ExportColumn<Customer>>
    {
        new("Ad", x => x.Name),
        new("Soyad / Firma", x => x.SurnameCompany),
        new("Telefon", x => x.Phone),
        new("Email", x => x.Email),
        new("Adres", x => x.Address)
    };
    }

}