namespace Nalbur.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }

    public decimal CostPrice { get; set; }
    public decimal SalePrice { get; set; }

    // Artýk stok ondalýklý olabilir: 2.5 kg, 1.75 litre, 12.50 m²
    public decimal CurrentStock { get; set; }

    // Minimum stok da ondalýklý olabilir
    public decimal MinimumStock { get; set; }

    // Adet, Kg, Litre, Metrekare vb.
    public string StockUnit { get; set; } = "Adet";

    public bool IsActive { get; set; } = true;
}