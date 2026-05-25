namespace InvenTrack.Application.DTOs.Reports;

public class InventorySummaryDto
{
    public int TotalProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int TotalCategories { get; set; }
    public int TotalSuppliers { get; set; }
}
