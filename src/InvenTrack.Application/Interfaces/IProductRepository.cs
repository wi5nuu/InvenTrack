using InvenTrack.Domain.Entities;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySkuAsync(string sku);
    Task<PagedResult<Product>> GetPagedWithIncludesAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? supplierId = null, bool? isLowStock = null);
    Task<IEnumerable<Product>> GetLowStockAsync();
}
