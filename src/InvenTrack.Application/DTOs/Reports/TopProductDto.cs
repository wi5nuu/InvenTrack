namespace InvenTrack.Application.DTOs.Reports;

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalMovements { get; set; }
    public int TotalQuantity { get; set; }
}
