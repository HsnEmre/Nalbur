using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly IProductService _productService;
    private List<Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    private string _searchText = string.Empty;
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterProducts();
            }
        }
    }

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private Product _newProduct = new();

    [ObservableProperty]
    private bool _isEditMode;

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value != null)
        {
            NewProduct = new Product
            {
                Id = value.Id,
                Code = value.Code,
                Barcode = value.Barcode,
                Name = value.Name,
                Category = value.Category,
                CostPrice = value.CostPrice,
                SalePrice = value.SalePrice,
                CurrentStock = value.CurrentStock,
                MinimumStock = value.MinimumStock,
                IsActive = value.IsActive
            };

            IsEditMode = true;
        }
        else
        {
            ClearForm();
        }
    }

    public ProductViewModel(IProductService productService)
    {
        _productService = productService;

        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
        SaveProductCommand = new AsyncRelayCommand(SaveProductAsync);
        DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync);
        ClearFormCommand = new RelayCommand(ClearForm);

        LoadProductsCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadProductsCommand { get; }
    public IAsyncRelayCommand SaveProductCommand { get; }
    public IAsyncRelayCommand DeleteProductCommand { get; }
    public IRelayCommand ClearFormCommand { get; }

    private async Task LoadProductsAsync()
    {
        var products = await _productService.GetAllAsync();

        _allProducts = products
            .OrderBy(x => x.Name)
            .ToList();

        FilterProducts();
    }

    private void FilterProducts()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Products = new ObservableCollection<Product>(_allProducts);
            return;
        }

        var search = SearchText.Trim();

        var filteredProducts = _allProducts
            .Where(p =>
                !string.IsNullOrWhiteSpace(p.Name) &&
                p.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .ToList();

        Products = new ObservableCollection<Product>(filteredProducts);
    }

    private async Task SaveProductAsync()
    {
        if (string.IsNullOrWhiteSpace(NewProduct.Name)) return;

        if (IsEditMode)
        {
            await _productService.UpdateAsync(NewProduct);
        }
        else
        {
            await _productService.AddAsync(NewProduct);
        }

        ClearForm();
        await LoadProductsAsync();
    }

    private async Task DeleteProductAsync()
    {
        if (SelectedProduct == null) return;

        if (System.Windows.MessageBox.Show(
                $"{SelectedProduct.Name} silinecek. Emin misiniz?",
                "Onay",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning) == System.Windows.MessageBoxResult.Yes)
        {
            await _productService.DeleteAsync(SelectedProduct.Id);
            ClearForm();
            await LoadProductsAsync();
        }
    }

    private void ClearForm()
    {
        NewProduct = new Product();
        IsEditMode = false;
        SelectedProduct = null;
    }
}