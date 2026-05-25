using InvenTrack.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvenTrack.API.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class ImportController : BaseApiController
{
    private readonly IImportService _importService;

    public ImportController(IImportService importService)
    {
        _importService = importService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var result = await _importService.UploadAsync(file, userId);
        return ApiResponseOk(result);
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs()
    {
        var result = await _importService.GetJobsAsync();
        return ApiResponseOk(result);
    }

    [HttpGet("jobs/{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var result = await _importService.GetJobByIdAsync(id);
        return ApiResponseOk(result);
    }

    [HttpGet("template/csv")]
    public IActionResult DownloadCsvTemplate()
    {
        var bytes = _importService.GenerateCsvTemplate();
        return File(bytes, "text/csv", "import-template.csv");
    }

    [HttpGet("template/excel")]
    public IActionResult DownloadExcelTemplate()
    {
        var bytes = _importService.GenerateExcelTemplate();
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "import-template.xlsx");
    }
}
