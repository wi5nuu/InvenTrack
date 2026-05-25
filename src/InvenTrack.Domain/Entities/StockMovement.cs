using InvenTrack.Domain.Enums;

namespace InvenTrack.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public MovementType MovementType { get; set; }
    public int Quantity { get; set; }
    public int QuantityBefore { get; set; }
    public int QuantityAfter { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
