using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nalbur.Domain.Entities;
using Nalbur.Domain.Enums;
using Nalbur.Domain.Interfaces;
using System.Collections.ObjectModel;

namespace Nalbur.Wpf.ViewModels;

public partial class OutgoingPaymentsViewModel : ViewModelBase
{
    private readonly IOutgoingPaymentService _paymentService;

    [ObservableProperty]
    private ObservableCollection<OutgoingPayment> _payments = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private OutgoingPayment? _selectedPayment;

    // Filters
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddMonths(-1);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today.AddMonths(2);

    [ObservableProperty]
    private string? _statusFilter = "Hepsi";

    [ObservableProperty]
    private string? _categoryFilter = "Hepsi";

    [ObservableProperty]
    private string? _searchText;

    // New Payment Form
    [ObservableProperty]
    private string _newTitle = string.Empty;

    [ObservableProperty]
    private string _newCategory = "Genel";

    [ObservableProperty]
    private decimal _newAmount;

    [ObservableProperty]
    private DateTime _newDueDate = DateTime.Today;

    [ObservableProperty]
    private string? _newDescription;

    public List<string> StatusOptions { get; } = new() { "Hepsi", "Bekliyor", "Ödenmiş", "Gecikti" };

    public OutgoingPaymentsViewModel(IOutgoingPaymentService paymentService)
    {
        _paymentService = paymentService;
        
        SearchCommand = new AsyncRelayCommand(LoadPaymentsAsync);
        AddPaymentCommand = new AsyncRelayCommand(AddPaymentAsync);
        MarkAsPaidCommand = new AsyncRelayCommand(MarkAsPaidAsync);

        Task.Run(async () => 
        {
            await LoadCategoriesAsync();
            await LoadPaymentsAsync();
        });
    }

    public IAsyncRelayCommand SearchCommand { get; }
    public IAsyncRelayCommand AddPaymentCommand { get; }
    public IAsyncRelayCommand MarkAsPaidCommand { get; }

    private async Task LoadCategoriesAsync()
    {
        var cats = await _paymentService.GetCategoriesAsync();
        var list = new List<string> { "Hepsi" };
        list.AddRange(cats);
        Categories = new ObservableCollection<string>(list);
    }

    private async Task LoadPaymentsAsync()
    {
        OutgoingPaymentStatus? status = StatusFilter switch
        {
            "Bekliyor" => OutgoingPaymentStatus.Pending,
            "Ödenmiş" => OutgoingPaymentStatus.Paid,
            "Gecikti" => OutgoingPaymentStatus.Overdue,
            _ => null
        };

        var results = await _paymentService.GetFilteredPaymentsAsync(StartDate, EndDate, status, CategoryFilter, SearchText);
        Payments = new ObservableCollection<OutgoingPayment>(results);
    }

    private async Task AddPaymentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewTitle) || NewAmount <= 0) return;

        var payment = new OutgoingPayment
        {
            Title = NewTitle,
            Category = NewCategory,
            Amount = NewAmount,
            DueDate = NewDueDate,
            Description = NewDescription,
            IsPaid = false,
            CreatedAt = DateTime.Now
        };

        await _paymentService.AddPaymentAsync(payment);
        
        // Reset form
        NewTitle = string.Empty;
        NewAmount = 0;
        NewDueDate = DateTime.Today;
        NewDescription = string.Empty;

        await LoadCategoriesAsync();
        await LoadPaymentsAsync();
    }

    private async Task MarkAsPaidAsync()
    {
        if (SelectedPayment == null || SelectedPayment.IsPaid) return;

        await _paymentService.MarkAsPaidAsync(SelectedPayment.Id);
        await LoadPaymentsAsync();
    }
}
