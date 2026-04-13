using CommunityToolkit.Mvvm.ComponentModel;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class DashboardViewModel : ViewModelBase
{
    private readonly IReminderService _reminderService;

    [ObservableProperty]
    private int _overdueCount;

    [ObservableProperty]
    private int _lowStockCount;

    public ObservableCollection<Installment> TodayDueInstallments { get; } = new();

    public DashboardViewModel(IReminderService reminderService)
    {
        _reminderService = reminderService;
        LoadDataAsync();
    }

    private async void LoadDataAsync()
    {
        OverdueCount = await _reminderService.GetOverdueCountAsync();
        LowStockCount = await _reminderService.GetLowStockCountAsync();
        
        var installments = await _reminderService.GetTodayDueInstallmentsAsync();
        TodayDueInstallments.Clear();
        foreach (var item in installments)
        {
            TodayDueInstallments.Add(item);
        }
    }
}
