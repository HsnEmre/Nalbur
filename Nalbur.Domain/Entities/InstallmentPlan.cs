namespace Nalbur.Domain.Entities;

public class InstallmentPlan : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal DownPayment { get; set; }
    public int InstallmentCount { get; set; }
    
    public ICollection<Installment> Installments { get; set; } = new List<Installment>();

    public decimal PaidAmount => Installments.Sum(i => i.PaidAmount);
    public decimal RemainingAmount => TotalAmount - PaidAmount - DownPayment;
    public int PaidCount => Installments.Count(i => i.Status == Nalbur.Domain.Enums.InstallmentStatus.Paid);
    public string ProgressDisplay => $"{PaidCount} / {InstallmentCount}";
}
