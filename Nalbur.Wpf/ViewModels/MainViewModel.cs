using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Nalbur.Wpf.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private string _title = "Nalbur Management System";

    public MainViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        NavigateToDashboardCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>());
        NavigateToProductsCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<ProductViewModel>());
        NavigateToCustomersCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<CustomerViewModel>());
        NavigateToSalesCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<SalesViewModel>());
        NavigateToInstallmentsCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<InstallmentViewModel>());
        NavigateToSalesHistoryCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<SalesHistoryViewModel>());
        NavigateToOutgoingPaymentsCommand = new RelayCommand(() => CurrentViewModel = _serviceProvider.GetRequiredService<OutgoingPaymentsViewModel>());

        // Default view
        CurrentViewModel = _serviceProvider.GetRequiredService<DashboardViewModel>();
    }

    public IRelayCommand NavigateToDashboardCommand { get; }
    public IRelayCommand NavigateToProductsCommand { get; }
    public IRelayCommand NavigateToCustomersCommand { get; }
    public IRelayCommand NavigateToSalesCommand { get; }
    public IRelayCommand NavigateToInstallmentsCommand { get; }
    public IRelayCommand NavigateToSalesHistoryCommand { get; }
    public IRelayCommand NavigateToOutgoingPaymentsCommand { get; }
}
