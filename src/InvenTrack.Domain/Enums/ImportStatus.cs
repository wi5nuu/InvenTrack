namespace InvenTrack.Domain.Enums;

/// <summary>ETL pipeline status for bulk CSV/Excel import jobs.</summary>
public enum ImportStatus
{
    Pending,
    Processing,
    Completed,
    Failed
}
