using System.Text.Json;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        var movements = await _dashboardService.GetMovementChartAsync(7);
        var categories = await _dashboardService.GetCategoryDistributionAsync();

        ViewBag.Summary = summary.Data;
        ViewBag.MovementsChart = System.Text.Json.JsonSerializer.Serialize(movements.Data);
        ViewBag.CategoriesChart = System.Text.Json.JsonSerializer.Serialize(categories.Data);

        return View();
    }
}
