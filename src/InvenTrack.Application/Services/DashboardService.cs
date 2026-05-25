using InvenTrack.Application.DTOs.Dashboard;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Domain.Enums;
using InvenTrack.Shared.Constants;
using InvenTrack.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace InvenTrack.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<DashboardService> logger)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardSummaryDto>> GetSummaryAsync()
    {
        if (_cache.TryGetValue(AppConstants.CacheKeys.DashboardSummary, out DashboardSummaryDto? cached))
            return ApiResponse<DashboardSummaryDto>.Ok(cached!);

        var products = await _unitOfWork.Products.GetAllAsync();
        var activeProducts = products.Where(p => p.IsActive).ToList();
        var recentMovements = await _unitOfWork.StockMovements.GetRecentMovementsAsync(7);
        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        var categories = await _unitOfWork.Categories.GetAllAsync();

        var summary = new DashboardSummaryDto
        {
            TotalProducts = activeProducts.Count,
            TotalStockValue = activeProducts.Sum(p => p.CostPrice * p.StockQuantity),
            LowStockItems = activeProducts.Count(p => p.StockQuantity <= p.MinStockThreshold && p.StockQuantity > 0),
            OutOfStockItems = activeProducts.Count(p => p.StockQuantity <= 0),
            RecentMovements = recentMovements.Count(),
            ActiveSuppliers = suppliers.Count(s => s.IsActive),
            TotalCategories = categories.Count(c => c.IsActive)
        };

        _cache.Set(AppConstants.CacheKeys.DashboardSummary, summary, TimeSpan.FromMinutes(5));

        return ApiResponse<DashboardSummaryDto>.Ok(summary);
    }

    public async Task<ApiResponse<ChartDataDto>> GetMovementChartAsync(int days = 7)
    {
        var movements = await _unitOfWork.StockMovements.GetRecentMovementsAsync(days);
        var grouped = movements
            .GroupBy(m => m.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new { Date = g.Key, In = g.Where(m => m.MovementType == MovementType.IN).Sum(m => m.Quantity), Out = g.Where(m => m.MovementType == MovementType.OUT).Sum(m => m.Quantity) })
            .ToList();

        var labels = grouped.Select(g => g.Date.ToString("MMM dd")).ToArray();
        var inData = grouped.Select(g => (decimal)g.In).ToArray();
        var outData = grouped.Select(g => (decimal)g.Out).ToArray();

        var chartData = new ChartDataDto
        {
            Labels = labels,
            Datasets = new[]
            {
                new ChartDatasetDto { Label = "Stock In", Data = inData, BackgroundColor = "#28a745", BorderColor = "#28a745" },
                new ChartDatasetDto { Label = "Stock Out", Data = outData, BackgroundColor = "#dc3545", BorderColor = "#dc3545" }
            }
        };

        return ApiResponse<ChartDataDto>.Ok(chartData);
    }

    public async Task<ApiResponse<ChartDataDto>> GetCategoryDistributionAsync()
    {
        var products = await _unitOfWork.Products.GetAllAsync();
        var activeProducts = products.Where(p => p.IsActive).ToList();

        var grouped = activeProducts
            .GroupBy(p => p.Category?.Name ?? "Uncategorized")
            .Select(g => new { Category = g.Key, Count = g.Sum(p => p.StockQuantity) })
            .ToList();

        var colors = new[] { "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8", "#6c757d", "#6610f2", "#e83e8c" };

        var chartData = new ChartDataDto
        {
            Labels = grouped.Select(g => g.Category).ToArray(),
            Datasets = new[]
            {
                new ChartDatasetDto
                {
                    Label = "Stock Quantity",
                    Data = grouped.Select(g => (decimal)g.Count).ToArray(),
                    BackgroundColor = string.Join(",", colors.Take(grouped.Count)),
                    BorderColor = "#ffffff"
                }
            }
        };

        return ApiResponse<ChartDataDto>.Ok(chartData);
    }
}
