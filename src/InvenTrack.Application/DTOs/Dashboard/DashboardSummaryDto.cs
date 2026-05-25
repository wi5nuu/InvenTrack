namespace InvenTrack.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalProducts { get; set; }
    public decimal TotalStockValue { get; set; }
    public int LowStockItems { get; set; }
    public int OutOfStockItems { get; set; }
    public int RecentMovements { get; set; }
    public int ActiveSuppliers { get; set; }
    public int TotalCategories { get; set; }
}
