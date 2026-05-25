using InvenTrack.Domain.Enums;

namespace InvenTrack.Domain.Entities;

public class ImportJob : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public string? ErrorLog { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
