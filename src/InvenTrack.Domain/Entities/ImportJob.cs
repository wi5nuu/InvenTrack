using InvenTrack.Domain.Enums;

namespace InvenTrack.Domain.Entities;

/// <summary>Tracks bulk CSV/Excel import jobs with processing status and error details.</summary>
public class ImportJob : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public FileType FileType { get; set; }
    public ImportStatus Status { get; set; } = ImportStatus.Pending;
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int FailedRows { get; set; }
    public string? ErrorMessage { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}
