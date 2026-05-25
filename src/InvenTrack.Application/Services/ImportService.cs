using System.Globalization;
using System.Text;
using AutoMapper;
using CsvHelper;
using CsvHelper.Configuration;
using InvenTrack.Application.DTOs.Import;
using InvenTrack.Application.Interfaces;
using InvenTrack.Domain.Entities;
using InvenTrack.Domain.Enums;
using InvenTrack.Shared.Constants;
using InvenTrack.Shared.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace InvenTrack.Application.Services;

public class ImportService : IImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ImportService> _logger;

    public ImportService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ImportService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<ImportJobDto>> UploadAsync(IFormFile file, int userId)
    {
        if (file == null || file.Length == 0)
            return ApiResponse<ImportJobDto>.Fail("No file uploaded");

        // SECURITY A08: Validate file size (max 10MB)
        if (file.Length > AppConstants.Import.MaxFileSizeMb * 1024 * 1024)
            return ApiResponse<ImportJobDto>.Fail($"File size exceeds {AppConstants.Import.MaxFileSizeMb}MB limit");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        // SECURITY A08: Validate file extension and magic bytes
        if (!AppConstants.Import.AllowedExtensions.Contains(extension))
            return ApiResponse<ImportJobDto>.Fail($"File type '{extension}' is not allowed. Use CSV or XLSX.");

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), AppConstants.Import.UploadPath);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var importJob = new ImportJob
        {
            FileName = file.FileName,
            FileType = extension == ".csv" ? FileType.CSV : FileType.Excel,
            Status = ImportStatus.Pending,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ImportJobs.AddAsync(importJob);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Import job created: {JobId} file: {FileName} by user {UserId}", importJob.Id, file.FileName, userId);

        var result = _mapper.Map<ImportJobDto>(importJob);
        return ApiResponse<ImportJobDto>.Ok(result, "File uploaded successfully. Import job queued.");
    }

    public async Task<ApiResponse<IEnumerable<ImportJobDto>>> GetJobsAsync()
    {
        var jobs = await _unitOfWork.ImportJobs.GetAllAsync();
        var dtos = _mapper.Map<IEnumerable<ImportJobDto>>(jobs.OrderByDescending(j => j.CreatedAt));
        return ApiResponse<IEnumerable<ImportJobDto>>.Ok(dtos);
    }

    public async Task<ApiResponse<ImportJobDto>> GetJobByIdAsync(int id)
    {
        var job = await _unitOfWork.ImportJobs.GetByIdAsync(id);
        if (job == null)
            return ApiResponse<ImportJobDto>.Fail("Import job not found");

        var dto = _mapper.Map<ImportJobDto>(job);
        return ApiResponse<ImportJobDto>.Ok(dto);
    }

    public byte[] GenerateCsvTemplate()
    {
        var sb = new StringBuilder();
        sb.AppendLine("SKU,ProductName,Category,Supplier,UnitPrice,CostPrice,InitialStock,MinStockThreshold,Unit,Description");
        sb.AppendLine("TECH-LAP-001,Sample Laptop,Electronics,TechSupply Co,1200.00,900.00,50,10,pcs,High-performance laptop");
        sb.AppendLine("OFFC-PPR-001,Printer Paper,Office Supplies,OfficeMart Inc,5.99,3.50,200,50,pcs,A4 printer paper 500 sheets");

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] GenerateExcelTemplate()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("ImportTemplate");

        var headers = new[] { "SKU", "ProductName", "Category", "Supplier", "UnitPrice", "CostPrice", "InitialStock", "MinStockThreshold", "Unit", "Description" };
        for (int i = 0; i < headers.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = headers[i];
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        worksheet.Cells[2, 1].Value = "TECH-LAP-001";
        worksheet.Cells[2, 2].Value = "Sample Laptop";
        worksheet.Cells[2, 3].Value = "Electronics";
        worksheet.Cells[2, 4].Value = "TechSupply Co";
        worksheet.Cells[2, 5].Value = 1200.00;
        worksheet.Cells[2, 6].Value = 900.00;
        worksheet.Cells[2, 7].Value = 50;
        worksheet.Cells[2, 8].Value = 10;
        worksheet.Cells[2, 9].Value = "pcs";
        worksheet.Cells[2, 10].Value = "High-performance laptop";

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    public async Task ProcessImportJobAsync(ImportJob job)
    {
        job.Status = ImportStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        var errors = new List<string>();
        int successCount = 0;
        int failCount = 0;

        try
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), AppConstants.Import.UploadPath, $"{Guid.NewGuid()}_{job.FileName}");
            // Find the actual file
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), AppConstants.Import.UploadPath);
            var actualFile = Directory.GetFiles(uploadDir, $"*{job.FileName}").FirstOrDefault()
                ?? Directory.GetFiles(uploadDir).FirstOrDefault(f => f.Contains(job.Id.ToString()));

            if (actualFile != null && File.Exists(actualFile))
            {
                if (job.FileType == FileType.CSV)
                    (successCount, failCount, errors) = await ProcessCsvFileAsync(actualFile, job.UserId);
                else
                    (successCount, failCount, errors) = await ProcessExcelFileAsync(actualFile, job.UserId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing import job {JobId}", job.Id);
            errors.Add($"Fatal error: {ex.Message}");
        }

        job.Status = failCount > 0 && successCount == 0 ? ImportStatus.Failed : ImportStatus.Completed;
        job.SuccessRows = successCount;
        job.FailedRows = failCount;
        job.ErrorLog = errors.Any() ? string.Join("\n", errors.Take(100)) : null;
        job.CompletedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Import job {JobId} completed: {Success} success, {Failed} failed", job.Id, successCount, failCount);
    }

    private async Task<(int success, int fail, List<string> errors)> ProcessCsvFileAsync(string filePath, int userId)
    {
        int success = 0, fail = 0;
        var errors = new List<string>();

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<dynamic>().ToList();
        var totalRows = records.Count;

        for (int i = 0; i < totalRows; i++)
        {
            try
            {
                var row = records[i] as IDictionary<string, object>;
                if (row == null) { fail++; continue; }

                var rowNum = i + 2; // +2 for header and 0-index
                var sku = GetString(row, "SKU");
                var productName = GetString(row, "ProductName");
                var categoryName = GetString(row, "Category");
                var supplierName = GetString(row, "Supplier");
                var unitPrice = GetDecimal(row, "UnitPrice");
                var costPrice = GetDecimal(row, "CostPrice");
                var initialStock = GetInt(row, "InitialStock");
                var minThreshold = GetInt(row, "MinStockThreshold");
                var unit = GetString(row, "Unit") ?? "pcs";
                var description = GetString(row, "Description");

                var validationError = ValidateRow(sku, productName, unitPrice, costPrice, initialStock, minThreshold);
                if (validationError != null)
                {
                    errors.Add($"Row {rowNum}: {validationError}");
                    fail++;
                    continue;
                }

                var category = await GetOrCreateCategoryAsync(categoryName ?? "Uncategorized");
                int? supplierId = null;
                if (!string.IsNullOrWhiteSpace(supplierName))
                {
                    var supplier = await GetOrCreateSupplierAsync(supplierName);
                    supplierId = supplier.Id;
                }

                var existingProduct = await _unitOfWork.Products.GetBySkuAsync(sku!);
                if (existingProduct != null)
                {
                    existingProduct.Name = productName!;
                    existingProduct.CategoryId = category.Id;
                    existingProduct.SupplierId = supplierId;
                    existingProduct.UnitPrice = unitPrice;
                    existingProduct.CostPrice = costPrice;
                    existingProduct.MinStockThreshold = minThreshold;
                    existingProduct.Unit = unit;
                    existingProduct.Description = description;
                    existingProduct.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(existingProduct);
                }
                else
                {
                    var product = new Product
                    {
                        SKU = sku!,
                        Name = productName!,
                        CategoryId = category.Id,
                        SupplierId = supplierId,
                        UnitPrice = unitPrice,
                        CostPrice = costPrice,
                        StockQuantity = initialStock,
                        MinStockThreshold = minThreshold,
                        Unit = unit,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Products.AddAsync(product);
                }

                success++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {i + 2}: {ex.Message}");
                fail++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (success, fail, errors);
    }

    private async Task<(int success, int fail, List<string> errors)> ProcessExcelFileAsync(string filePath, int userId)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        int success = 0, fail = 0;
        var errors = new List<string>();

        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (int row = 2; row <= rowCount; row++)
        {
            try
            {
                var sku = worksheet.Cells[row, 1].Text;
                var productName = worksheet.Cells[row, 2].Text;
                var categoryName = worksheet.Cells[row, 3].Text;
                var supplierName = worksheet.Cells[row, 4].Text;
                var unitPrice = worksheet.Cells[row, 5].GetValue<decimal>();
                var costPrice = worksheet.Cells[row, 6].GetValue<decimal>();
                var initialStock = worksheet.Cells[row, 7].GetValue<int>();
                var minThreshold = worksheet.Cells[row, 8].GetValue<int>();
                var unit = string.IsNullOrWhiteSpace(worksheet.Cells[row, 9].Text) ? "pcs" : worksheet.Cells[row, 9].Text;
                var description = worksheet.Cells[row, 10].Text;

                var validationError = ValidateRow(sku, productName, unitPrice, costPrice, initialStock, minThreshold);
                if (validationError != null)
                {
                    errors.Add($"Row {row}: {validationError}");
                    fail++;
                    continue;
                }

                var category = await GetOrCreateCategoryAsync(categoryName);
                int? supplierId = null;
                if (!string.IsNullOrWhiteSpace(supplierName))
                {
                    var supplier = await GetOrCreateSupplierAsync(supplierName);
                    supplierId = supplier.Id;
                }

                var existingProduct = await _unitOfWork.Products.GetBySkuAsync(sku);
                if (existingProduct != null)
                {
                    existingProduct.Name = productName;
                    existingProduct.CategoryId = category.Id;
                    existingProduct.SupplierId = supplierId;
                    existingProduct.UnitPrice = unitPrice;
                    existingProduct.CostPrice = costPrice;
                    existingProduct.MinStockThreshold = minThreshold;
                    existingProduct.Unit = unit;
                    existingProduct.Description = description;
                    existingProduct.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(existingProduct);
                }
                else
                {
                    var product = new Product
                    {
                        SKU = sku,
                        Name = productName,
                        CategoryId = category.Id,
                        SupplierId = supplierId,
                        UnitPrice = unitPrice,
                        CostPrice = costPrice,
                        StockQuantity = initialStock,
                        MinStockThreshold = minThreshold,
                        Unit = unit,
                        Description = description,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Products.AddAsync(product);
                }

                success++;
            }
            catch (Exception ex)
            {
                errors.Add($"Row {row}: {ex.Message}");
                fail++;
            }
        }

        await _unitOfWork.SaveChangesAsync();
        return (success, fail, errors);
    }

    private static string? ValidateRow(string? sku, string? productName, decimal unitPrice, decimal costPrice, int initialStock, int minThreshold)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return "SKU is required";
        if (sku.Length > 50)
            return "SKU must not exceed 50 characters";
        if (string.IsNullOrWhiteSpace(productName))
            return "Product name is required";
        if (productName.Length > 200)
            return "Product name must not exceed 200 characters";
        if (unitPrice <= 0 || unitPrice > 999999999)
            return "Unit price must be > 0 and < 999999999";
        if (costPrice <= 0 || costPrice > 999999999)
            return "Cost price must be > 0 and < 999999999";
        if (costPrice > unitPrice)
            return "Cost price must be <= unit price";
        if (initialStock < 0)
            return "Initial stock must be >= 0";
        if (minThreshold < 0)
            return "Min stock threshold must be >= 0";

        return null;
    }

    private async Task<Category> GetOrCreateCategoryAsync(string name)
    {
        var category = await _unitOfWork.Categories.GetByNameAsync(name);
        if (category == null)
        {
            category = new Category { Name = name, CreatedAt = DateTime.UtcNow };
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();
        }
        return category;
    }

    private async Task<Supplier> GetOrCreateSupplierAsync(string name)
    {
        var supplier = await _unitOfWork.Suppliers.GetByNameAsync(name);
        if (supplier == null)
        {
            supplier = new Supplier { Name = name, CreatedAt = DateTime.UtcNow };
            await _unitOfWork.Suppliers.AddAsync(supplier);
            await _unitOfWork.SaveChangesAsync();
        }
        return supplier;
    }

    private static string? GetString(IDictionary<string, object> row, string key)
    {
        if (row.TryGetValue(key, out var val) && val != null)
            return val.ToString();
        var altKey = row.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
        if (altKey != null && row[altKey] != null)
            return row[altKey]?.ToString();
        return null;
    }

    private static decimal GetDecimal(IDictionary<string, object> row, string key)
    {
        var val = GetString(row, key);
        return decimal.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }

    private static int GetInt(IDictionary<string, object> row, string key)
    {
        var val = GetString(row, key);
        return int.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out var result) ? result : 0;
    }
}
