namespace InvenTrack.Application.DTOs.Products;

public class CreateProductDto
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal CostPrice { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockThreshold { get; set; }
    public string Unit { get; set; } = "pcs";
    public string? ImageUrl { get; set; }
}
