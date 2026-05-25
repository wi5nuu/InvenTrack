using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize]
public class DashboardController : BaseApiController
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _dashboardService.GetSummaryAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("chart/movements")]
    public async Task<IActionResult> GetMovementChart([FromQuery] int days = 7)
    {
        var result = await _dashboardService.GetMovementChartAsync(days);
        return ApiResponseOk(result);
    }

    [HttpGet("chart/categories")]
    public async Task<IActionResult> GetCategoryChart()
    {
        var result = await _dashboardService.GetCategoryDistributionAsync();
        return ApiResponseOk(result);
    }
}
