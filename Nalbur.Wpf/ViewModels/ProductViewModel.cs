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

    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportWordCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }

    public List<string> UnitOptions { get; } = new()
{
    "Adet",
    "Kg",
    "Gram",
    "Litre",
    "Metrekare",
    "Metre",
    "Paket",
    "Kutu",
    "Çuval",
    "Teneke",
    "Rulo"
};

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
                StockUnit = string.IsNullOrWhiteSpace(value.StockUnit) ? "Adet" : value.StockUnit,
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
        ExportExcelCommand = new RelayCommand(() =>
    ExportHelper.ExportToExcel("Ürün Listesi", Products, ProductColumns()));

        ExportWordCommand = new RelayCommand(() =>
            ExportHelper.ExportToWord("Ürün Listesi", Products, ProductColumns()));

        ExportPdfCommand = new RelayCommand(() =>
            ExportHelper.ExportToPdf("Ürün Listesi", Products, ProductColumns()));

        LoadProductsCommand.Execute(null);
    }


    public IAsyncRelayCommand LoadProductsCommand { get; }
    public IAsyncRelayCommand SaveProductCommand { get; }
    public IAsyncRelayCommand DeleteProductCommand { get; }
    public IRelayCommand ClearFormCommand { get; }



    private static List<ExportColumn<Product>> ProductColumns()
    {
        return new List<ExportColumn<Product>>
    {
        new("Kod", x => x.Code),
        new("Barkod", x => x.Barcode),
        new("Ürün Adý", x => x.Name),
        new("Kategori", x => x.Category),
        new("Stok", x => x.CurrentStock),
        new("Birim", x => x.StockUnit),
        new("Minimum Stok", x => x.MinimumStock),
        new("Alýþ Fiyatý", x => x.CostPrice),
        new("Satýþ Fiyatý", x => x.SalePrice),
        new("Aktif", x => x.IsActive)
    };
    }


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
        NewProduct = new Product
        {
            StockUnit = "Adet",
            IsActive = true
        };

        IsEditMode = false;
        SelectedProduct = null;
    }
}