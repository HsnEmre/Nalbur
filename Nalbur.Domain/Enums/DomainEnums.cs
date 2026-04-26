namespace Nalbur.Domain.Enums;

public enum SaleType
{
    Cash,
    Card,
    Mixed,
    Installment
}

public enum InstallmentStatus
{
    Pending,
    Paid,
    Overdue,
    Cancelled 
}

public enum StockTransactionType
{
    In,
    Out
}

public enum OutgoingPaymentStatus
{
    Pending,
    Paid,
    Overdue
}
