using AutoMapper;
using InvenTrack.Application.DTOs.Categories;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using InvenTrack.Shared.Constants;

namespace InvenTrack.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache, ILogger<CategoryService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync()
    {
        if (_cache.TryGetValue(AppConstants.CacheKeys.ProductCategories, out IEnumerable<CategoryDto>? cached))
            return ApiResponse<IEnumerable<CategoryDto>>.Ok(cached!);

        var categories = await _unitOfWork.Categories.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<CategoryDto>>(categories).ToList();

        foreach (var dto in dtos)
        {
            var category = categories.First(c => c.Id == dto.Id);
            dto.ProductCount = category.Products?.Count(p => p.IsActive) ?? 0;
        }

        _cache.Set(AppConstants.CacheKeys.ProductCategories, dtos, TimeSpan.FromMinutes(10));

        return ApiResponse<IEnumerable<CategoryDto>>.Ok(dtos.AsEnumerable());
    }

    public async Task<ApiResponse<CategoryDto>> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ApiResponse<CategoryDto>.Fail("Category not found");

        var dto = _mapper.Map<CategoryDto>(category);
        dto.ProductCount = category.Products?.Count(p => p.IsActive) ?? 0;

        return ApiResponse<CategoryDto>.Ok(dto);
    }

    public async Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto)
    {
        var existing = await _unitOfWork.Categories.GetByNameAsync(dto.Name);
        if (existing != null)
            return ApiResponse<CategoryDto>.Fail($"Category '{dto.Name}' already exists");

        var category = _mapper.Map<Category>(dto);
        category.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(AppConstants.CacheKeys.ProductCategories);
        _logger.LogInformation("Category created: {Name}", dto.Name);

        var result = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(result, "Category created successfully");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateAsync(int id, CreateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ApiResponse<CategoryDto>.Fail("Category not found");

        var existing = await _unitOfWork.Categories.GetByNameAsync(dto.Name);
        if (existing != null && existing.Id != id)
            return ApiResponse<CategoryDto>.Fail($"Category '{dto.Name}' already exists");

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(AppConstants.CacheKeys.ProductCategories);
        _logger.LogInformation("Category updated: {Id}", id);

        var result = _mapper.Map<CategoryDto>(category);
        return ApiResponse<CategoryDto>.Ok(result, "Category updated successfully");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category == null)
            return ApiResponse<string>.Fail("Category not found");

        if (category.Products?.Any(p => p.IsActive) == true)
            return ApiResponse<string>.Fail("Cannot delete category with active products");

        category.IsActive = false;
        category.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(AppConstants.CacheKeys.ProductCategories);

        return ApiResponse<string>.Ok("Category deleted successfully");
    }
}
