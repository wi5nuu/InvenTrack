namespace InvenTrack.Application.DTOs.Reports;

public class StockMovementReportDto
{
    public int MovementId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reference { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
