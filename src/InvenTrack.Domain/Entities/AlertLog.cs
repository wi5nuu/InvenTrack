using InvenTrack.Domain.Enums;

namespace InvenTrack.Domain.Entities;

public class AlertLog : BaseEntity
{
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public AlertType AlertType { get; set; }
    public bool EmailSent { get; set; }
    public string? SentTo { get; set; }
    public DateTime? SentAt { get; set; }
}
