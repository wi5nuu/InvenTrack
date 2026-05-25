using InvenTrack.Application.DTOs.StockMovements;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface IStockMovementService
{
    Task<PagedResult<StockMovementDto>> GetPagedAsync(int page, int pageSize, int? productId = null, string? type = null, DateTime? from = null, DateTime? to = null);
    Task<ApiResponse<StockMovementDto>> CreateAsync(CreateStockMovementDto dto, int userId);
}
