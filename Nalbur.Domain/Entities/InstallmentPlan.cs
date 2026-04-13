namespace Nalbur.Domain.Entities;

public class InstallmentPlan : BaseEntity
{
    public int SaleId { get; set; }
    public Sale Sale { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal DownPayment { get; set; }
    public int InstallmentCount { get; set; }
    
    public ICollection<Installment> Installments { get; set; } = new List<Installment>();
}
