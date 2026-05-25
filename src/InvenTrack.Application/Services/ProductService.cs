using AutoMapper;
using InvenTrack.Application.DTOs.Products;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Domain.Exceptions;
using InvenTrack.Shared.Wrappers;
using Microsoft.Extensions.Logging;

namespace InvenTrack.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ProductDto>> GetPagedAsync(int page, int pageSize, string? search = null, int? categoryId = null, int? supplierId = null, bool? isLowStock = null)
    {
        var pagedResult = await _unitOfWork.Products.GetPagedWithIncludesAsync(page, pageSize, search, categoryId, supplierId, isLowStock);
        var items = _mapper.Map<IEnumerable<ProductDto>>(pagedResult.Items);
        return new PagedResult<ProductDto>(items, pagedResult.TotalCount, page, pageSize);
    }

    public async Task<ApiResponse<ProductDto>> GetByIdAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.Fail("Product not found");

        var dto = _mapper.Map<ProductDto>(product);
        return ApiResponse<ProductDto>.Ok(dto);
    }

    public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductDto dto, int userId)
    {
        var existing = await _unitOfWork.Products.GetBySkuAsync(dto.SKU);
        if (existing != null)
            return ApiResponse<ProductDto>.Fail($"Product with SKU '{dto.SKU}' already exists");

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (category == null)
            return ApiResponse<ProductDto>.Fail("Category not found");

        var product = _mapper.Map<Product>(dto);
        product.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        if (dto.StockQuantity > 0)
        {
            await _unitOfWork.StockMovements.AddAsync(new StockMovement
            {
                ProductId = product.Id,
                MovementType = Domain.Enums.MovementType.IN,
                Quantity = dto.StockQuantity,
                QuantityBefore = 0,
                QuantityAfter = dto.StockQuantity,
                Reference = "Initial stock",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _unitOfWork.SaveChangesAsync();
        }

        _logger.LogInformation("Product created: {Sku} by user {UserId}", dto.SKU, userId);

        var result = _mapper.Map<ProductDto>(product);
        return ApiResponse<ProductDto>.Ok(result, "Product created successfully");
    }

    public async Task<ApiResponse<ProductDto>> UpdateAsync(int id, UpdateProductDto dto, int userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.Fail("Product not found");

        var existingBySku = await _unitOfWork.Products.GetBySkuAsync(dto.SKU);
        if (existingBySku != null && existingBySku.Id != id)
            return ApiResponse<ProductDto>.Fail($"SKU '{dto.SKU}' is already in use by another product");

        _mapper.Map(dto, product);
        product.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product updated: {Id} by user {UserId}", id, userId);

        var result = _mapper.Map<ProductDto>(product);
        return ApiResponse<ProductDto>.Ok(result, "Product updated successfully");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<string>.Fail("Product not found");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        return ApiResponse<string>.Ok("Product deactivated successfully");
    }

    public async Task<PagedResult<ProductDto>> GetLowStockAsync(int page, int pageSize)
    {
        return await GetPagedAsync(page, pageSize, isLowStock: true);
    }

    public async Task<ApiResponse<ProductDto>> AdjustStockAsync(int id, AdjustStockDto dto, int userId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
            return ApiResponse<ProductDto>.Fail("Product not found");

        var quantityBefore = product.StockQuantity;
        product.StockQuantity = dto.Quantity;
        product.UpdatedAt = DateTime.UtcNow;

        var movementType = dto.Quantity >= quantityBefore ? Domain.Enums.MovementType.ADJUSTMENT : Domain.Enums.MovementType.OUT;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.StockMovements.AddAsync(new StockMovement
        {
            ProductId = id,
            MovementType = movementType,
            Quantity = Math.Abs(dto.Quantity - quantityBefore),
            QuantityBefore = quantityBefore,
            QuantityAfter = dto.Quantity,
            Reference = dto.Notes ?? "Manual adjustment",
            Notes = dto.Notes,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        });
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Stock adjusted for product {Id}: {Before} -> {After} by user {UserId}", id, quantityBefore, dto.Quantity, userId);

        var result = _mapper.Map<ProductDto>(product);
        return ApiResponse<ProductDto>.Ok(result, "Stock adjusted successfully");
    }
}
