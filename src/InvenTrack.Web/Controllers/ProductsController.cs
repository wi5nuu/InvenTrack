using System.Security.Claims;
using InvenTrack.Application.DTOs.Products;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ISupplierService _supplierService;

    public ProductsController(IProductService productService, ICategoryService categoryService, ISupplierService supplierService)
    {
        _productService = productService;
        _categoryService = categoryService;
        _supplierService = supplierService;
    }

    public async Task<IActionResult> Index(int page = 1, string? search = null)
    {
        var result = await _productService.GetPagedAsync(page, 10, search);
        ViewBag.Search = search;
        return View(result);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = (await _categoryService.GetAllAsync()).Data;
        ViewBag.Suppliers = (await _supplierService.GetAllAsync()).Data;
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create(CreateProductDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _productService.CreateAsync(dto, userId);

        if (result.Success)
            return RedirectToAction("Index");

        ViewBag.Categories = (await _categoryService.GetAllAsync()).Data;
        ViewBag.Suppliers = (await _supplierService.GetAllAsync()).Data;
        ViewBag.Error = result.Message;
        return View(dto);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var result = await _productService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
            return RedirectToAction("Index");

        ViewBag.Categories = (await _categoryService.GetAllAsync()).Data;
        ViewBag.Suppliers = (await _supplierService.GetAllAsync()).Data;
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Edit(int id, UpdateProductDto dto)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var result = await _productService.UpdateAsync(id, dto, userId);

        if (result.Success)
            return RedirectToAction("Index");

        ViewBag.Categories = (await _categoryService.GetAllAsync()).Data;
        ViewBag.Suppliers = (await _supplierService.GetAllAsync()).Data;
        ViewBag.Error = result.Message;
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Import()
    {
        return View();
    }
}
