using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("inventory-summary")]
    public async Task<IActionResult> GetInventorySummary()
    {
        var result = await _reportService.GetInventorySummaryAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock()
    {
        var result = await _reportService.GetLowStockProductsAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetMovements([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int? productId = null, [FromQuery] string? type = null)
    {
        var result = await _reportService.GetStockMovementReportAsync(from, to, productId, type);
        return ApiResponseOk(result);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null, [FromQuery] int top = 10)
    {
        var result = await _reportService.GetTopMovingProductsAsync(from, to, top);
        return ApiResponseOk(result);
    }

    [HttpGet("valuation")]
    public async Task<IActionResult> GetValuation()
    {
        var result = await _reportService.GetInventoryValuationAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("export/pdf")]
    public IActionResult ExportPdf()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userId = int.TryParse(userIdClaim, out var id) ? id : 0;
        var bytes = _reportService.GeneratePdfReport(userId);
        return File(bytes, "application/pdf", $"inventory-report-{DateTime.Now:yyyyMMdd}.pdf");
    }

    [HttpGet("export/excel")]
    public IActionResult ExportExcel()
    {
        var bytes = _reportService.GenerateExcelReport();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"inventory-report-{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
