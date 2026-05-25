using InvenTrack.Application.DTOs.StockMovements;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize]
public class StockMovementsController : BaseApiController
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] int? productId = null, [FromQuery] string? type = null, [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
    {
        var result = await _stockMovementService.GetPagedAsync(page, pageSize, productId, type, from, to);
        return Ok(new
        {
            success = true,
            data = result.Items,
            pagination = new { page, pageSize, totalCount = result.TotalCount, totalPages = result.TotalPages }
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _stockMovementService.CreateAsync(dto, userId);
        return ApiResponseOk(result);
    }
}
