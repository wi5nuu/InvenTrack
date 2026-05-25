using InvenTrack.Domain.Entities;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface IStockMovementRepository : IRepository<StockMovement>
{
    Task<PagedResult<StockMovement>> GetPagedWithFiltersAsync(int page, int pageSize, int? productId = null, string? type = null, DateTime? from = null, DateTime? to = null);
    Task<IEnumerable<StockMovement>> GetRecentMovementsAsync(int days = 7);
}
