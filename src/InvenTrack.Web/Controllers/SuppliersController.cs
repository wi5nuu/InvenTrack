using InvenTrack.Application.DTOs.Suppliers;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ISupplierService _supplierService;

    public SuppliersController(ISupplierService supplierService)
    {
        _supplierService = supplierService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _supplierService.GetAllAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CreateSupplierDto dto)
    {
        await _supplierService.CreateAsync(dto);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(int id, CreateSupplierDto dto)
    {
        await _supplierService.UpdateAsync(id, dto);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _supplierService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}
