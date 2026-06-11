using InvenTrack.Application.DTOs.Dashboard;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

/// <summary>Dashboard data aggregation — summary stats, stock movement chart, category distribution.</summary>
public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<ChartDataDto>> GetMovementChartAsync(int days = 7);
    Task<ApiResponse<ChartDataDto>> GetCategoryDistributionAsync();
}
