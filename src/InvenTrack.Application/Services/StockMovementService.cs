using AutoMapper;
using InvenTrack.Application.DTOs.StockMovements;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Shared.Wrappers;
using Microsoft.Extensions.Logging;

namespace InvenTrack.Application.Services;

public class StockMovementService : IStockMovementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<StockMovementService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<StockMovementDto>> GetPagedAsync(int page, int pageSize, int? productId = null, string? type = null, DateTime? from = null, DateTime? to = null)
    {
        var pagedResult = await _unitOfWork.StockMovements.GetPagedWithFiltersAsync(page, pageSize, productId, type, from, to);
        var items = _mapper.Map<IEnumerable<StockMovementDto>>(pagedResult.Items);
        return new PagedResult<StockMovementDto>(items, pagedResult.TotalCount, page, pageSize);
    }

    public async Task<ApiResponse<StockMovementDto>> CreateAsync(CreateStockMovementDto dto, int userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
        if (product == null)
            return ApiResponse<StockMovementDto>.Fail("Product not found");

        var quantityBefore = product.StockQuantity;
        int newQuantity;

        switch (dto.MovementType)
        {
            case Domain.Enums.MovementType.IN:
                newQuantity = quantityBefore + dto.Quantity;
                break;
            case Domain.Enums.MovementType.OUT:
                if (quantityBefore < dto.Quantity)
                    return ApiResponse<StockMovementDto>.Fail("Insufficient stock");
                newQuantity = quantityBefore - dto.Quantity;
                break;
            default:
                return ApiResponse<StockMovementDto>.Fail("Invalid movement type");
        }

        product.StockQuantity = newQuantity;
        product.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Products.Update(product);

        var movement = new StockMovement
        {
            ProductId = dto.ProductId,
            MovementType = dto.MovementType,
            Quantity = dto.Quantity,
            QuantityBefore = quantityBefore,
            QuantityAfter = newQuantity,
            Reference = dto.Reference,
            Notes = dto.Notes,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.StockMovements.AddAsync(movement);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Stock movement created: {Type} {Qty} of product {ProductId} by user {UserId}",
            dto.MovementType, dto.Quantity, dto.ProductId, userId);

        var result = _mapper.Map<StockMovementDto>(movement);
        return ApiResponse<StockMovementDto>.Ok(result, "Stock movement recorded");
    }
}
