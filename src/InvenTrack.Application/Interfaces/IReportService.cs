using InvenTrack.Application.DTOs.Reports;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface IReportService
{
    Task<ApiResponse<InventorySummaryDto>> GetInventorySummaryAsync();
    Task<ApiResponse<IEnumerable<LowStockReportDto>>> GetLowStockProductsAsync();
    Task<ApiResponse<IEnumerable<StockMovementReportDto>>> GetStockMovementReportAsync(DateTime? from = null, DateTime? to = null, int? productId = null, string? type = null);
    Task<ApiResponse<IEnumerable<TopProductDto>>> GetTopMovingProductsAsync(DateTime? from = null, DateTime? to = null, int top = 10);
    Task<ApiResponse<IEnumerable<CategoryValuationDto>>> GetInventoryValuationAsync();
    byte[] GeneratePdfReport(int userId);
    byte[] GenerateExcelReport();
}
