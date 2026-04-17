using Nalbur.Domain.Enums;

namespace Nalbur.Domain.Entities;

public class OutgoingPayment : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Category { get; set; } = "Genel";
    public bool IsPaid { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public OutgoingPaymentStatus Status
    {
        get
        {
            if (IsPaid) return OutgoingPaymentStatus.Paid;
            if (DateTime.Today > DueDate) return OutgoingPaymentStatus.Overdue;
            return OutgoingPaymentStatus.Pending;
        }
    }
}
