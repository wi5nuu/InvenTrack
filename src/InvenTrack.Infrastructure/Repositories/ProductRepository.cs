using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;
using InvenTrack.Shared.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetBySkuAsync(string sku)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.SKU == sku);
    }

    public async Task<PagedResult<Product>> GetPagedWithIncludesAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? supplierId = null, bool? isLowStock = null)
    {
        var query = _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.SKU.Contains(search) ||
                (p.Category != null && p.Category.Name.Contains(search)));
        }

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (supplierId.HasValue)
            query = query.Where(p => p.SupplierId == supplierId.Value);

        if (isLowStock.HasValue && isLowStock.Value)
            query = query.Where(p => p.StockQuantity <= p.MinStockThreshold);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive && p.StockQuantity <= p.MinStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _dbSet
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }
}
