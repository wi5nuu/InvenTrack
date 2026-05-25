namespace InvenTrack.Application.DTOs.Reports;

public class LowStockReportDto
{
    public int ProductId { get; set; }
    public string SKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public string? SupplierEmail { get; set; }
    public string? SupplierPhone { get; set; }
    public int StockQuantity { get; set; }
    public int MinStockThreshold { get; set; }
    public decimal UnitPrice { get; set; }
}
