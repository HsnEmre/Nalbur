using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;
using System.Windows;

namespace Nalbur.Wpf.ViewModels;

public partial class WorkContractViewModel : ViewModelBase
{
    private readonly IWorkContractService _contractService;

    [ObservableProperty]
    private ObservableCollection<WorkContract> _contracts = new();

    [ObservableProperty]
    private WorkContract? _selectedContract;

    [ObservableProperty]
    private WorkContract _newContract = new()
    {
        ContractDate = DateTime.Today
    };

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private DateTime? _startDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime? _endDate = DateTime.Today;

    [ObservableProperty]
    private string? _searchText;

    partial void OnSelectedContractChanged(WorkContract? value)
    {
        if (value != null)
        {
            NewContract = new WorkContract
            {
                Id = value.Id,
                Title = value.Title,
                CustomerName = value.CustomerName,
                CustomerPhone = value.CustomerPhone,
                WorkDescription = value.WorkDescription,
                Materials = value.Materials,
                Notes = value.Notes,
                ContractDate = value.ContractDate,
                CreatedAt = value.CreatedAt,
                UpdatedAt = value.UpdatedAt
            };

            IsEditMode = true;
        }
        else
        {
            ClearForm();
        }
    }

    public WorkContractViewModel(IWorkContractService contractService)
    {
        _contractService = contractService;

        LoadContractsCommand = new AsyncRelayCommand(LoadContractsAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        SaveContractCommand = new AsyncRelayCommand(SaveContractAsync);
        DeleteContractCommand = new AsyncRelayCommand(DeleteContractAsync);
        ClearFormCommand = new RelayCommand(ClearForm);

        ExportExcelCommand = new RelayCommand(() =>
            ExportHelper.ExportToExcel("Sözleşmeler", Contracts, ContractColumns()));

        ExportWordCommand = new RelayCommand(() =>
            ExportHelper.ExportToWord("Sözleşmeler", Contracts, ContractColumns()));

        ExportPdfCommand = new RelayCommand(() =>
            ExportHelper.ExportToPdf("Sözleşmeler", Contracts, ContractColumns()));

        LoadContractsCommand.Execute(null);
    }

    public IAsyncRelayCommand LoadContractsCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand SaveContractCommand { get; }
    public IAsyncRelayCommand DeleteContractCommand { get; }
    public IRelayCommand ClearFormCommand { get; }

    public IRelayCommand ExportExcelCommand { get; }
    public IRelayCommand ExportWordCommand { get; }
    public IRelayCommand ExportPdfCommand { get; }

    private async Task LoadContractsAsync()
    {
        var result = await _contractService.GetAllAsync();
        Contracts = new ObservableCollection<WorkContract>(result);
    }

    private async Task SearchAsync()
    {
        var result = await _contractService.GetFilteredAsync(StartDate, EndDate, SearchText);
        Contracts = new ObservableCollection<WorkContract>(result);
    }

    private async Task SaveContractAsync()
    {
        if (string.IsNullOrWhiteSpace(NewContract.Title))
        {
            MessageBox.Show(
                "Sözleşme başlığı boş olamaz.",
                "Uyarı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewContract.WorkDescription))
        {
            MessageBox.Show(
                "Yapılacak işler boş olamaz.",
                "Uyarı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(NewContract.Materials))
        {
            MessageBox.Show(
                "Kullanılacak malzemeler boş olamaz.",
                "Uyarı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (IsEditMode)
        {
            await _contractService.UpdateAsync(NewContract);
        }
        else
        {
            await _contractService.AddAsync(NewContract);
        }

        ClearForm();
        await SearchAsync();
    }

    private async Task DeleteContractAsync()
    {
        if (SelectedContract == null)
            return;

        var result = MessageBox.Show(
            $"{SelectedContract.Title} silinecek. Emin misiniz?",
            "Onay",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        await _contractService.DeleteAsync(SelectedContract.Id);

        ClearForm();
        await SearchAsync();
    }

    private void ClearForm()
    {
        NewContract = new WorkContract
        {
            ContractDate = DateTime.Today
        };

        IsEditMode = false;
        SelectedContract = null;
    }

    private static List<ExportColumn<WorkContract>> ContractColumns()
    {
        return new List<ExportColumn<WorkContract>>
        {
            new("Tarih", x => x.ContractDate),
            new("Başlık", x => x.Title),
            new("Müşteri", x => x.CustomerName),
            new("Telefon", x => x.CustomerPhone),
            new("Yapılacak İşler", x => x.WorkDescription),
            new("Malzemeler", x => x.Materials),
            new("Notlar", x => x.Notes),
            new("Oluşturma Tarihi", x => x.CreatedAt),
            new("Güncelleme Tarihi", x => x.UpdatedAt)
        };
    }
}