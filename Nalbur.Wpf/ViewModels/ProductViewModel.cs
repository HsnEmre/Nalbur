using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IProductService _productService;

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private Product _newProduct = new();

    public ProductViewModel(IProductService productService)
    {
        _productService = productService;
        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
        SaveProductCommand = new AsyncRelayCommand(SaveProductAsync);
        
        LoadProductsCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadProductsCommand { get; }
    public IAsyncRelayCommand SaveProductCommand { get; }

    private async Task LoadProductsAsync()
    {
        var products = await _productService.GetAllAsync();
        Products = new ObservableCollection<Product>(products);
    }

    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProduct.Name)) return;

        await _productService.AddAsync(NewProduct);
        NewProduct = new Product();
        await LoadProductsAsync();
    }
}
