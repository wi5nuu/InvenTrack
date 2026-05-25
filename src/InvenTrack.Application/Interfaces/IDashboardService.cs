using InvenTrack.Application.DTOs.Dashboard;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync();
    Task<ApiResponse<ChartDataDto>> GetMovementChartAsync(int days = 7);
    Task<ApiResponse<ChartDataDto>> GetCategoryDistributionAsync();
}
