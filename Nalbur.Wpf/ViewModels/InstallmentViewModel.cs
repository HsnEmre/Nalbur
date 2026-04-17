using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class InstallmentViewModel : ViewModelBase
{
    private readonly IInstallmentService _installmentService;

    [ObservableProperty]
    private ObservableCollection<Installment> _installments = new();

    [ObservableProperty]
    private Installment? _selectedInstallment;

    [ObservableProperty]
    private decimal _paymentAmount;

    [ObservableProperty]
    private ObservableCollection<SaleItem> _selectedSaleItems = new();

    [ObservableProperty]
    private Sale? _selectedSale;

    partial void OnSelectedInstallmentChanged(Installment? value)
    {
        if (value != null)
        {
            PaymentAmount = value.RemainingAmount;
            SelectedSale = value.InstallmentPlan?.Sale;
            SelectedSaleItems = new ObservableCollection<SaleItem>(value.InstallmentPlan?.Sale?.SaleItems ?? new List<SaleItem>());
        }
        else
        {
            SelectedSale = null;
            SelectedSaleItems.Clear();
        }
    }

    public InstallmentViewModel(IInstallmentService installmentService)
    {
        _installmentService = installmentService;
        RefreshCommand = new AsyncRelayCommand(LoadInstallmentsAsync);
        PayInstallmentCommand = new AsyncRelayCommand(PayInstallmentAsync);
        
        RefreshCommand.Execute(null);
    }

    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand PayInstallmentCommand { get; }

    private async Task LoadInstallmentsAsync()
    {
        var allActive = await _installmentService.GetActiveInstallmentsAsync();
        Installments = new ObservableCollection<Installment>(allActive);
    }

    private async Task PayInstallmentAsync()
    {
        if (SelectedInstallment == null || PaymentAmount <= 0) return;

        await _installmentService.ProcessPaymentAsync(SelectedInstallment.Id, PaymentAmount);
        await LoadInstallmentsAsync();
        
        // Clear selection to reset
        SelectedInstallment = null;
        PaymentAmount = 0;
    }
}
