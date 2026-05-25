namespace InvenTrack.Application.DTOs.Import;

public class ImportJobDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorLog { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
