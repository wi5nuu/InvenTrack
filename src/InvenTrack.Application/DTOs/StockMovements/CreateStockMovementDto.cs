using InvenTrack.Domain.Enums;

namespace InvenTrack.Application.DTOs.StockMovements;

public class CreateStockMovementDto
{
    public int ProductId { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
