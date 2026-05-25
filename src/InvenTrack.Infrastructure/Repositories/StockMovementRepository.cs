using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Infrastructure.Data;
using InvenTrack.Shared.Wrappers;
using Microsoft.EntityFrameworkCore;

namespace InvenTrack.Infrastructure.Repositories;

public class StockMovementRepository : Repository<StockMovement>, IStockMovementRepository
{
    public StockMovementRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<PagedResult<StockMovement>> GetPagedWithFiltersAsync(int page, int pageSize, int? productId = null, string? type = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _dbSet
            .Include(m => m.Product)
            .Include(m => m.User)
            .AsQueryable();

        if (productId.HasValue)
            query = query.Where(m => m.ProductId == productId.Value);

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(m => m.MovementType.ToString() == type);

        if (from.HasValue)
            query = query.Where(m => m.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(m => m.CreatedAt <= to.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<StockMovement>(items, totalCount, page, pageSize);
    }

    public async Task<IEnumerable<StockMovement>> GetRecentMovementsAsync(int days = 7)
    {
        var since = DateTime.UtcNow.AddDays(-days);
        return await _dbSet
            .Include(m => m.Product)
            .Include(m => m.User)
            .Where(m => m.CreatedAt >= since)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }
}
