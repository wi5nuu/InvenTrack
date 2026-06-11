using InvenTrack.Domain.Enums;

namespace InvenTrack.Domain.Entities;

/// <summary>Records inbound/outbound stock changes for audit trail and inventory tracking.</summary>
public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public decimal? UnitPriceAtMovement { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
