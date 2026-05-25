namespace InvenTrack.Application.DTOs.Dashboard;

public class ChartDataDto
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public ChartDatasetDto[] Datasets { get; set; } = Array.Empty<ChartDatasetDto>();
}

public class ChartDatasetDto
{
    public string Label { get; set; } = string.Empty;
    public decimal[] Data { get; set; } = Array.Empty<decimal>();
    public string? BackgroundColor { get; set; }
    public string? BorderColor { get; set; }
}
