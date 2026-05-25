using InvenTrack.Application.DTOs.Categories;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.Web.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _categoryService.GetAllAsync();
        return View(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        await _categoryService.CreateAsync(dto);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CreateCategoryDto dto)
    {
        await _categoryService.UpdateAsync(id, dto);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return RedirectToAction("Index");
    }
}
