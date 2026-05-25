using InvenTrack.Application.DTOs.Categories;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface ICategoryService
{
    Task<ApiResponse<IEnumerable<CategoryDto>>> GetAllAsync();
    Task<ApiResponse<CategoryDto>> GetByIdAsync(int id);
    Task<ApiResponse<CategoryDto>> CreateAsync(CreateCategoryDto dto);
    Task<ApiResponse<CategoryDto>> UpdateAsync(int id, CreateCategoryDto dto);
    Task<ApiResponse<string>> DeleteAsync(int id);
}
