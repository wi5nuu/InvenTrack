using InvenTrack.Application.DTOs.Suppliers;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize]
public class SuppliersController : BaseApiController
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _supplierService.GetAllAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _supplierService.GetByIdAsync(id);
        return ApiResponseOk(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateSupplierDto dto)
    {
        var result = await _supplierService.CreateAsync(dto);
        return ApiResponseOk(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateSupplierDto dto)
    {
        var result = await _supplierService.UpdateAsync(id, dto);
        return ApiResponseOk(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _supplierService.DeleteAsync(id);
        return ApiResponseOk(result);
    }
}
