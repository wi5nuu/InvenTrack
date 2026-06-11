using InvenTrack.Application.DTOs.Products;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

/// <summary>Product management service with pagination, search, filtering, and stock adjustment.</summary>
public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? supplierId = null, bool? isLowStock = null);
    Task<ApiResponse<ProductDto>> GetByIdAsync(int id);
    Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto, int userId);
    Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, int userId);
    Task<ApiResponse<string>> DeleteAsync(int id);
    Task<PagedResult<ProductDto>> GetLowStockAsync(int page, int pageSize);
    Task<ApiResponse<ProductDto>> AdjustStockAsync(int id, AdjustStockDto dto, int userId);
}
