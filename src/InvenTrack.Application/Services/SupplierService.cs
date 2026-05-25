using AutoMapper;
using InvenTrack.Application.DTOs.Suppliers;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Shared.Constants;
using InvenTrack.Shared.Wrappers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace InvenTrack.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, IMemoryCache cache, ILogger<SupplierService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ApiResponse<IEnumerable<SupplierDto>>> GetAllAsync()
    {
        if (_cache.TryGetValue(AppConstants.CacheKeys.Suppliers, out IEnumerable<SupplierDto>? cached))
            return ApiResponse<IEnumerable<SupplierDto>>.Ok(cached!);

        var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<SupplierDto>>(suppliers).ToList();

        foreach (var dto in dtos)
        {
            var supplier = suppliers.First(s => s.Id == dto.Id);
            dto.ProductCount = supplier.Products?.Count(p => p.IsActive) ?? 0;
        }

        _cache.Set(AppConstants.CacheKeys.Suppliers, dtos, TimeSpan.FromMinutes(10));

        return ApiResponse<IEnumerable<SupplierDto>>.Ok(dtos.AsEnumerable());
    }

    public async Task<ApiResponse<SupplierDto>> GetByIdAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
            return ApiResponse<SupplierDto>.Fail("Supplier not found");

        var dto = _mapper.Map<SupplierDto>(supplier);
        dto.ProductCount = supplier.Products?.Count(p => p.IsActive) ?? 0;

        return ApiResponse<SupplierDto>.Ok(dto);
    }

    public async Task<ApiResponse<SupplierDto>> CreateAsync(CreateSupplierDto dto)
    {
        var existing = await _unitOfWork.Suppliers.GetByNameAsync(dto.Name);
        if (existing != null)
            return ApiResponse<SupplierDto>.Fail($"Supplier '{dto.Name}' already exists");

        var supplier = _mapper.Map<Supplier>(dto);
        supplier.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Suppliers.AddAsync(supplier);
        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(AppConstants.CacheKeys.Suppliers);
        _logger.LogInformation("Supplier created: {Name}", dto.Name);

        var result = _mapper.Map<SupplierDto>(supplier);
        return ApiResponse<SupplierDto>.Ok(result, "Supplier created successfully");
    }

    public async Task<ApiResponse<SupplierDto>> UpdateAsync(int id, CreateSupplierDto dto)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
            return ApiResponse<SupplierDto>.Fail("Supplier not found");

        var existing = await _unitOfWork.Suppliers.GetByNameAsync(dto.Name);
        if (existing != null && existing.Id != id)
            return ApiResponse<SupplierDto>.Fail($"Supplier '{dto.Name}' already exists");

        _mapper.Map(dto, supplier);
        supplier.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(AppConstants.CacheKeys.Suppliers);
        _logger.LogInformation("Supplier updated: {Id}", id);

        var result = _mapper.Map<SupplierDto>(supplier);
        return ApiResponse<SupplierDto>.Ok(result, "Supplier updated successfully");
    }

    public async Task<ApiResponse<string>> DeleteAsync(int id)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
        if (supplier == null)
            return ApiResponse<string>.Fail("Supplier not found");

        if (supplier.Products?.Any(p => p.IsActive) == true)
            return ApiResponse<string>.Fail("Cannot delete supplier with active products");

        supplier.IsActive = false;
        supplier.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Suppliers.Update(supplier);
        await _unitOfWork.SaveChangesAsync();

        _cache.Remove(AppConstants.CacheKeys.Suppliers);

        return ApiResponse<string>.Ok("Supplier deleted successfully");
    }
}
