using InvenTrack.Application.DTOs.Suppliers;
using InvenTrack.Shared.Wrappers;

namespace InvenTrack.Application.Interfaces;

public interface ISupplierService
{
    Task<ApiResponse<IEnumerable<SupplierDto>>> GetAllAsync();
    Task<ApiResponse<SupplierDto>> GetByIdAsync(int id);
    Task<ApiResponse<SupplierDto>> CreateAsync(CreateSupplierDto dto);
    Task<ApiResponse<SupplierDto>> UpdateAsync(int id, CreateSupplierDto dto);
    Task<ApiResponse<string>> DeleteAsync(int id);
}
