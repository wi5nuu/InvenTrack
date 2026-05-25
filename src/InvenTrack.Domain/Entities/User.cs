namespace InvenTrack.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Staff;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public bool IsActive { get; set; } = true;
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
    public ICollection<ImportJob> ImportJobs { get; set; } = new List<ImportJob>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
