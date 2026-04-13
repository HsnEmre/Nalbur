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

    public InstallmentViewModel(IInstallmentService installmentService)
    {
        _installmentService = installmentService;
        LoadInstallmentsCommand = new AsyncRelayCommand(LoadInstallmentsAsync);
        PayInstallmentCommand = new AsyncRelayCommand(PayInstallmentAsync);

        LoadInstallmentsCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadInstallmentsCommand { get; }
    public IAsyncRelayCommand PayInstallmentCommand { get; }

    private async Task LoadInstallmentsAsync()
    {
        var due = await _installmentService.GetUpcomingInstallmentsAsync(30);
        var overdue = await _installmentService.GetOverdueInstallmentsAsync();
        
        var all = due.Concat(overdue).OrderBy(i => i.DueDate).ToList();
        Installments = new ObservableCollection<Installment>(all);
    }

    private async Task PayInstallmentAsync()
    {
        if (SelectedInstallment == null) return;

        await _installmentService.MarkAsPaidAsync(SelectedInstallment.Id);
        await LoadInstallmentsAsync();
    }
}
