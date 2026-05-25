namespace InvenTrack.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}
