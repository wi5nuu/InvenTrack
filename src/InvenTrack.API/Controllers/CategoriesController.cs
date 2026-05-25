using InvenTrack.Application.DTOs.Categories;
using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return ApiResponseOk(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return ApiResponseOk(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        return ApiResponseOk(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return ApiResponseOk(result);
    }
}
