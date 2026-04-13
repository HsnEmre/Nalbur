using Nalbur.Domain.Enums;

namespace Nalbur.Domain.Entities;

public class Sale : BaseEntity
{
    public DateTime SaleDate { get; set; } = DateTime.Now;
    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public decimal TotalAmount { get; set; }
    public SaleType SaleType { get; set; }
    
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();
    public InstallmentPlan? InstallmentPlan { get; set; }
}
