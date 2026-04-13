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
    Overdue
}

public enum StockTransactionType
{
    In,
    Out
}
