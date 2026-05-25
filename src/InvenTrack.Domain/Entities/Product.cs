namespace InvenTrack.Domain.Entities;

public class Product : BaseEntity
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockThreshold { get; set; }
    public string Unit { get; set; } = "pcs";
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<AlertLog> AlertLogs { get; set; } = new List<AlertLog>();
}
