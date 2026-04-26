using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class SalesViewModel : ViewModelBase
{
    private readonly ISaleService _saleService;
    private readonly IProductService _productService;
    private readonly ICustomerService _customerService;
    private bool _syncingDiscount;

    [ObservableProperty]
    private ObservableCollection<Product> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _filteredCustomers = new();

    private string _customerSearchText = string.Empty;
    public string CustomerSearchText
    {
        get => _customerSearchText;
        set
        {
            if (SetProperty(ref _customerSearchText, value))
            {
                FilterCustomers();
            }
        }
    }

    [ObservableProperty]
    private ObservableCollection<SaleItem> _cartItems = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    partial void OnSelectedCustomerChanged(Customer? value)
    {
        if (value != null)
        {
            CustomerSearchText = $"{value.Name} {value.SurnameCompany}".Trim();
        }
    }

    [ObservableProperty]
    private SaleType _selectedSaleType = SaleType.Cash;

    [ObservableProperty]
    private decimal _subTotalAmount;

    [ObservableProperty]
    private decimal _discountAmount;

    partial void OnDiscountAmountChanged(decimal value)
    {
        if (_syncingDiscount) return;

        if (value < 0)
        {
            DiscountAmount = 0;
            return;
        }

        if (SubTotalAmount > 0 && value > SubTotalAmount)
        {
            DiscountAmount = SubTotalAmount;
            return;
        }

        _syncingDiscount = true;
        DiscountPercent = SubTotalAmount > 0
            ? Math.Round((DiscountAmount / SubTotalAmount) * 100, 2)
            : 0;
        _syncingDiscount = false;

        CalculateTotal();
    }

    [ObservableProperty]
    private decimal _discountPercent;

    partial void OnDiscountPercentChanged(decimal value)
    {
        if (_syncingDiscount) return;

        if (value < 0)
        {
            DiscountPercent = 0;
            return;
        }

        if (value > 100)
        {
            DiscountPercent = 100;
            return;
        }

        _syncingDiscount = true;
        DiscountAmount = Math.Round(SubTotalAmount * DiscountPercent / 100, 2);
        _syncingDiscount = false;

        CalculateTotal();
    }

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private int _quantityToAdd = 1;

    partial void OnQuantityToAddChanged(int value)
    {
        if (value < 1)
        {
            QuantityToAdd = 1;
        }
    }

    [ObservableProperty]
    private int _installmentCount = 3;

    [ObservableProperty]
    private decimal _downPayment;

    public SalesViewModel(ISaleService saleService, IProductService productService, ICustomerService customerService)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;

        AddToCartCommand = new RelayCommand<object>(AddToCart);
        RemoveFromCartCommand = new RelayCommand<object>(RemoveFromCart);
        ProcessSaleCommand = new AsyncRelayCommand(ProcessSaleAsync);
        ClearCartCommand = new RelayCommand(ClearCart);

        LoadDataAsync();
    }

    public IRelayCommand<object> AddToCartCommand { get; }
    public IRelayCommand<object> RemoveFromCartCommand { get; }
    public IAsyncRelayCommand ProcessSaleCommand { get; }
    public IRelayCommand ClearCartCommand { get; }

    private async void LoadDataAsync()
    {
        var products = await _productService.GetAllAsync();
        AvailableProducts = new ObservableCollection<Product>(products.Where(p => p.IsActive));

        var customers = await _customerService.GetAllAsync();
        Customers = new ObservableCollection<Customer>(customers);
        FilteredCustomers = new ObservableCollection<Customer>(customers);
    }

    private void ClearCart()
    {
        CartItems.Clear();
        DiscountAmount = 0;
        DiscountPercent = 0;
        CalculateTotal();
        SelectedCustomer = null;
        CustomerSearchText = string.Empty;
        SelectedSaleType = SaleType.Cash;
        DownPayment = 0;
        InstallmentCount = 3;
        QuantityToAdd = 1;
    }

    private void FilterCustomers()
    {
        if (string.IsNullOrWhiteSpace(CustomerSearchText))
        {
            FilteredCustomers = new ObservableCollection<Customer>(Customers);
            return;
        }

        var lowerSearch = CustomerSearchText.ToLower();
        var filtered = Customers.Where(c =>
            c.Name.ToLower().Contains(lowerSearch) ||
            (c.SurnameCompany?.ToLower().Contains(lowerSearch) ?? false) ||
            (c.Phone?.Contains(lowerSearch) ?? false));

        FilteredCustomers = new ObservableCollection<Customer>(filtered);
    }

    private void AddToCart(object? parameter)
    {
        if (parameter is not Product product) return;

        int quantity = QuantityToAdd < 1 ? 1 : QuantityToAdd;

        if (product.CurrentStock < quantity)
        {
            System.Windows.MessageBox.Show(
                $"Yetersiz stok. Mevcut stok: {product.CurrentStock}",
                "Stok Uyarýsý",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);

        if (existingItem != null)
        {
            var newQuantity = existingItem.Quantity + quantity;

            if (product.CurrentStock < newQuantity)
            {
                System.Windows.MessageBox.Show(
                    $"Yetersiz stok. Mevcut stok: {product.CurrentStock}, sepetteki mevcut adet: {existingItem.Quantity}",
                    "Stok Uyarýsý",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            existingItem.Quantity = newQuantity;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            CartItems.Add(new SaleItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = quantity,
                UnitPrice = product.SalePrice,
                TotalPrice = quantity * product.SalePrice
            });
        }

        CalculateTotal();
    }

    private void RemoveFromCart(object? parameter)
    {
        if (parameter is SaleItem item)
        {
            CartItems.Remove(item);
            CalculateTotal();
        }
    }

    private void CalculateTotal()
    {
        SubTotalAmount = CartItems.Sum(i => i.TotalPrice);

        _syncingDiscount = true;

        if (SubTotalAmount <= 0)
        {
            DiscountAmount = 0;
            DiscountPercent = 0;
        }
        else if (DiscountAmount > SubTotalAmount)
        {
            DiscountAmount = SubTotalAmount;
            DiscountPercent = 100;
        }
        else
        {
            DiscountPercent = Math.Round((DiscountAmount / SubTotalAmount) * 100, 2);
        }

        _syncingDiscount = false;

        TotalAmount = Math.Max(0, SubTotalAmount - DiscountAmount);
    }

    private async Task ProcessSaleAsync()
    {
        if (!CartItems.Any()) return;

        if (SelectedSaleType == SaleType.Installment && SelectedCustomer == null)
        {
            System.Windows.MessageBox.Show(
                "Taksitli satýþ için müþteri seçmelisiniz.",
                "Uyarý",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (SelectedSaleType == SaleType.Installment)
        {
            if (InstallmentCount < 1)
            {
                System.Windows.MessageBox.Show(
                    "Taksit sayýsý en az 1 olmalýdýr.",
                    "Uyarý",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (DownPayment < 0)
            {
                System.Windows.MessageBox.Show(
                    "Peþinat negatif olamaz.",
                    "Uyarý",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            if (DownPayment > TotalAmount)
            {
                System.Windows.MessageBox.Show(
                    "Peþinat, iskonto sonrasý toplam tutardan büyük olamaz.",
                    "Uyarý",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }
        }

        var sale = new Sale
        {
            CustomerId = SelectedCustomer?.Id,
            TotalAmount = TotalAmount,
            SaleType = SelectedSaleType,
            SaleItems = CartItems.ToList()
        };

        InstallmentPlan? plan = null;
        if (SelectedSaleType == SaleType.Installment)
        {
            plan = new InstallmentPlan
            {
                TotalAmount = TotalAmount,
                DownPayment = DownPayment,
                InstallmentCount = InstallmentCount
            };

            decimal installmentAmount = (TotalAmount - DownPayment) / InstallmentCount;
            for (int i = 1; i <= InstallmentCount; i++)
            {
                plan.Installments.Add(new Installment
                {
                    Amount = installmentAmount,
                    DueDate = DateTime.Today.AddMonths(i),
                    Status = InstallmentStatus.Pending
                });
            }
        }

        await _saleService.ProcessSaleAsync(sale, plan);

        ClearCart();
    }
}