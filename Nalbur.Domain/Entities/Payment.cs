using System;

namespace Nalbur.Domain.Entities;

public class Payment : BaseEntity
{
    public int InstallmentId { get; set; }
    public Installment Installment { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Now;
    public string? Note { get; set; }
}
