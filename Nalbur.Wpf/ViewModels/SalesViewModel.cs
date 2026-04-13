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

    [ObservableProperty]
    private ObservableCollection<Product> _availableProducts = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private ObservableCollection<SaleItem> _cartItems = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private SaleType _selectedSaleType = SaleType.Cash;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private int _installmentCount = 3;

    [ObservableProperty]
    private decimal _downPayment;

    public SalesViewModel(ISaleService saleService, IProductService productService, ICustomerService customerService)
    {
        _saleService = saleService;
        _productService = productService;
        _customerService = customerService;

        AddToCartCommand = new RelayCommand<Product>(AddToCart);
        RemoveFromCartCommand = new RelayCommand<SaleItem>(RemoveFromCart);
        ProcessSaleCommand = new AsyncRelayCommand(ProcessSaleAsync);

        LoadDataAsync();
    }

    public IRelayCommand<Product> AddToCartCommand { get; }
    public IRelayCommand<SaleItem> RemoveFromCartCommand { get; }
    public IAsyncRelayCommand ProcessSaleCommand { get; }

    private async void LoadDataAsync()
    {
        var products = await _productService.GetAllAsync();
        AvailableProducts = new ObservableCollection<Product>(products.Where(p => p.IsActive));

        var customers = await _customerService.GetAllAsync();
        Customers = new ObservableCollection<Customer>(customers);
    }

    private void AddToCart(Product? product)
    {
        if (product == null) return;

        var existingItem = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity++;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice;
        }
        else
        {
            CartItems.Add(new SaleItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = 1,
                UnitPrice = product.SalePrice,
                TotalPrice = product.SalePrice
            });
        }
        CalculateTotal();
    }

    private void RemoveFromCart(SaleItem? item)
    {
        if (item != null)
        {
            CartItems.Remove(item);
            CalculateTotal();
        }
    }

    private void CalculateTotal()
    {
        TotalAmount = CartItems.Sum(i => i.TotalPrice);
    }

    private async Task ProcessSaleAsync()
    {
        if (!CartItems.Any()) return;

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

            // Generate dummy installments for simplicity in this MVP
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
        
        // Reset
        CartItems.Clear();
        TotalAmount = 0;
        SelectedCustomer = null;
        SelectedSaleType = SaleType.Cash;
    }
}
