using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

[Authorize]
public class StockMovementsController : Controller
{
    private readonly IStockMovementService _stockMovementService;

    public StockMovementsController(IStockMovementService stockMovementService)
    {
        _stockMovementService = stockMovementService;
    }

    public async Task<IActionResult> Index(int page = 1, int? productId = null, string? type = null, DateTime? from = null, DateTime? to = null)
    {
        var result = await _stockMovementService.GetPagedAsync(page, 20, productId, type, from, to);
        ViewBag.ProductId = productId;
        ViewBag.Type = type;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(result);
    }
}
