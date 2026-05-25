using InvenTrack.Application.DTOs.Import;
using InvenTrack.Shared.Wrappers;
using Microsoft.AspNetCore.Http;

namespace InvenTrack.Application.Interfaces;

public interface IImportService
{
    Task<ApiResponse<ImportJobDto>> UploadAsync(IFormFile file, int userId);
    Task<ApiResponse<IEnumerable<ImportJobDto>>> GetJobsAsync();
    Task<ApiResponse<ImportJobDto>> GetJobByIdAsync(int id);
    byte[] GenerateCsvTemplate();
    byte[] GenerateExcelTemplate();
}
