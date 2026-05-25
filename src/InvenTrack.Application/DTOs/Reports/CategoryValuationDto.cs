namespace InvenTrack.Application.DTOs.Reports;

public class CategoryValuationDto
{
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
    public int TotalStock { get; set; }
    public decimal TotalStockValue { get; set; }
}
