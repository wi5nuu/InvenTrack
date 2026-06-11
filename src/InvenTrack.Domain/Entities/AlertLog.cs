using InvenTrack.Domain.Enums;

namespace InvenTrack.Domain.Entities;

/// <summary>Stores low-stock and system alert notifications sent to administrators.</summary>
public class AlertLog : BaseEntity
{
    public AlertType AlertType { get; set; }
    public string? Message { get; set; }
    public string? SentTo { get; set; }
    public bool IsSent { get; set; }
    public int? ProductId { get; set; }
    public Product? Product { get; set; }
}
