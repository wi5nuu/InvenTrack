namespace InvenTrack.Application.DTOs.StockMovements;

public class StockMovementDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public string MovementType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
