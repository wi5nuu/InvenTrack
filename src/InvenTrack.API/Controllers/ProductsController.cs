using InvenTrack.Application.DTOs.Products;
using InvenTrack.Application.Interfaces;
using InvenTrack.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, [FromQuery] int? categoryId = null, [FromQuery] int? supplierId = null)
    {
        var result = await _productService.GetPagedAsync(page, pageSize, search, categoryId, supplierId);
        return Ok(new
        {
            success = true,
            data = result.Items,
            pagination = new { page, pageSize, totalCount = result.TotalCount, totalPages = result.TotalPages }
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        return ApiResponseOk(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _productService.CreateAsync(dto, userId);
        return ApiResponseOk(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _productService.UpdateAsync(id, dto, userId);
        return ApiResponseOk(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _productService.DeleteAsync(id);
        return ApiResponseOk(result);
    }

    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStock([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _productService.GetLowStockAsync(page, pageSize);
        return Ok(new
        {
            success = true,
            data = result.Items,
            pagination = new { page, pageSize, totalCount = result.TotalCount, totalPages = result.TotalPages }
        });
    }

    [HttpPatch("{id}/stock")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] AdjustStockDto dto)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _productService.AdjustStockAsync(id, dto, userId);
        return ApiResponseOk(result);
    }
}
