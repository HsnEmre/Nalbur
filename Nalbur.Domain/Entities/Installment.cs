using Nalbur.Domain.Enums;

namespace Nalbur.Domain.Entities;

public class Installment : BaseEntity
{
    public int InstallmentPlanId { get; set; }
    public InstallmentPlan InstallmentPlan { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public InstallmentStatus Status { get; set; } = InstallmentStatus.Pending;
    public DateTime? PaymentDate { get; set; }
}
