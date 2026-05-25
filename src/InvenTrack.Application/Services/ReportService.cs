using System.Data;
using Dapper;
using InvenTrack.Application.DTOs.Reports;
using InvenTrack.Application.Interfaces;
using InvenTrack.Shared.Wrappers;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging;

namespace InvenTrack.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ReportService> _logger;

    public ReportService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ReportService> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<ApiResponse<InventorySummaryDto>> GetInventorySummaryAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);

        var result = await connection.QuerySingleAsync<InventorySummaryDto>(
            "EXEC sp_GetInventorySummary",
            commandType: CommandType.Text);

        return ApiResponse<InventorySummaryDto>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<LowStockReportDto>>> GetLowStockProductsAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);

        var result = await connection.QueryAsync<LowStockReportDto>(
            "EXEC sp_GetLowStockProducts",
            commandType: CommandType.Text);

        return ApiResponse<IEnumerable<LowStockReportDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<StockMovementReportDto>>> GetStockMovementReportAsync(
        DateTime? from = null, DateTime? to = null, int? productId = null, string? type = null)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);

        var result = await connection.QueryAsync<StockMovementReportDto>(
            "EXEC sp_GetStockMovementReport @FromDate, @ToDate, @ProductId, @Type",
            new { FromDate = from, ToDate = to, ProductId = productId, Type = type });

        return ApiResponse<IEnumerable<StockMovementReportDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<TopProductDto>>> GetTopMovingProductsAsync(
        DateTime? from = null, DateTime? to = null, int top = 10)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);

        var result = await connection.QueryAsync<TopProductDto>(
            "EXEC sp_GetTopMovingProducts @FromDate, @ToDate, @Top",
            new { FromDate = from, ToDate = to, Top = top });

        return ApiResponse<IEnumerable<TopProductDto>>.Ok(result);
    }

    public async Task<ApiResponse<IEnumerable<CategoryValuationDto>>> GetInventoryValuationAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        await using var connection = new SqlConnection(connectionString);

        var result = await connection.QueryAsync<CategoryValuationDto>(
            "EXEC sp_GetInventoryValuation",
            commandType: CommandType.Text);

        return ApiResponse<IEnumerable<CategoryValuationDto>>.Ok(result);
    }

    public byte[] GeneratePdfReport(int userId)
    {
        var products = _unitOfWork.Products.GetAllAsync().Result;
        var summary = GetInventorySummaryAsync().Result.Data;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Header().Element(c => ComposeHeader(c, summary));
                page.Content().Element(c => ComposeContent(c, products));
                page.Footer().Element(ComposeFooter);
            });
        });

        return document.GeneratePdf();
    }

    private void ComposeHeader(IContainer container, InventorySummaryDto? summary)
    {
        container.Column(column =>
        {
            column.Item().Row(row =>
            {
                row.RelativeItem().Text("InvenTrack Enterprise").FontSize(20).Bold();
                row.RelativeItem().AlignRight().Text($"Report Date: {DateTime.Now:dd MMM yyyy HH:mm}").FontSize(10);
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Blue.Medium);

            if (summary != null)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(5).Text($"Total Products: {summary.TotalProducts}").FontSize(11);
                    row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(5).Text($"Total Value: {summary.TotalStockValue:C}").FontSize(11);
                    row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(5).Text($"Low Stock: {summary.LowStockCount}").FontSize(11);
                    row.RelativeItem().Background(Colors.Grey.Lighten4).Padding(5).Text($"Out of Stock: {summary.OutOfStockCount}").FontSize(11);
                });
            }

            column.Item().PaddingVertical(3);
        });
    }

    private void ComposeContent(IContainer container, IEnumerable<Domain.Entities.Product> products)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("SKU").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Name").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Category").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Unit Price").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Cost Price").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Stock").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Threshold").FontColor(Colors.White).FontSize(9).Bold();
                header.Cell().Background(Colors.Blue.Darken2).Padding(2).Text("Value").FontColor(Colors.White).FontSize(9).Bold();
            });

            foreach (var product in products.OrderBy(p => p.Category?.Name).ThenBy(p => p.Name))
            {
                var bgColor = product.StockQuantity <= 0 ? Colors.Red.Lighten4 :
                    product.StockQuantity <= product.MinStockThreshold ? Colors.Orange.Lighten4 : Colors.White;

                table.Cell().Background(bgColor).Padding(2).Text(product.SKU).FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text(product.Name).FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text(product.Category?.Name ?? "").FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text(product.UnitPrice.ToString("C")).FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text(product.CostPrice.ToString("C")).FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text(product.StockQuantity.ToString()).FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text(product.MinStockThreshold.ToString()).FontSize(8);
                table.Cell().Background(bgColor).Padding(2).Text((product.CostPrice * product.StockQuantity).ToString("C")).FontSize(8);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span($"Generated by InvenTrack | {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(8).FontColor(Colors.Grey.Darken2);
            });
            row.RelativeItem().AlignRight().Text(text =>
            {
                text.CurrentPageNumber().FontSize(8);
                text.Span(" / ").FontSize(8);
                text.TotalPages().FontSize(8);
            });
        });
    }

    public byte[] GenerateExcelReport()
    {
        var products = _unitOfWork.Products.GetAllAsync().Result;
        var summary = GetInventorySummaryAsync().Result.Data;

        using var workbook = new XLWorkbook();

        // Summary sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        summarySheet.Cell(1, 1).Value = "Inventory Summary Report";
        summarySheet.Cell(1, 1).Style.Font.Bold = true;
        summarySheet.Cell(1, 1).Style.Font.FontSize = 14;

        if (summary != null)
        {
            summarySheet.Cell(3, 1).Value = "Metric";
            summarySheet.Cell(3, 2).Value = "Value";
            summarySheet.Range(3, 1, 3, 2).Style.Font.Bold = true;
            summarySheet.Cell(4, 1).Value = "Total Products";
            summarySheet.Cell(4, 2).Value = summary.TotalProducts;
            summarySheet.Cell(5, 1).Value = "Total Stock Value";
            summarySheet.Cell(5, 2).Value = summary.TotalStockValue;
            summarySheet.Cell(5, 2).Style.NumberFormat.Format = "$#,##0.00";
            summarySheet.Cell(6, 1).Value = "Low Stock Items";
            summarySheet.Cell(6, 2).Value = summary.LowStockCount;
            summarySheet.Cell(7, 1).Value = "Out of Stock Items";
            summarySheet.Cell(7, 2).Value = summary.OutOfStockCount;
        }

        summarySheet.Columns().AdjustToContents();

        // Full Inventory sheet
        var inventorySheet = workbook.Worksheets.Add("Full Inventory");
        var headers = new[] { "SKU", "Name", "Category", "Supplier", "Unit Price", "Cost Price", "Stock", "Threshold", "Unit", "Value" };
        for (int i = 0; i < headers.Length; i++)
        {
            inventorySheet.Cell(1, i + 1).Value = headers[i];
            inventorySheet.Cell(1, i + 1).Style.Font.Bold = true;
            inventorySheet.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.Blue;
            inventorySheet.Cell(1, i + 1).Style.Font.FontColor = XLColor.White;
        }

        int row = 2;
        foreach (var product in products.OrderBy(p => p.Category?.Name).ThenBy(p => p.Name))
        {
            inventorySheet.Cell(row, 1).Value = product.SKU;
            inventorySheet.Cell(row, 2).Value = product.Name;
            inventorySheet.Cell(row, 3).Value = product.Category?.Name ?? "";
            inventorySheet.Cell(row, 4).Value = product.Supplier?.Name ?? "";
            inventorySheet.Cell(row, 5).Value = product.UnitPrice;
            inventorySheet.Cell(row, 5).Style.NumberFormat.Format = "$#,##0.00";
            inventorySheet.Cell(row, 6).Value = product.CostPrice;
            inventorySheet.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
            inventorySheet.Cell(row, 7).Value = product.StockQuantity;
            inventorySheet.Cell(row, 8).Value = product.MinStockThreshold;
            inventorySheet.Cell(row, 9).Value = product.Unit;
            inventorySheet.Cell(row, 10).Value = product.CostPrice * product.StockQuantity;
            inventorySheet.Cell(row, 10).Style.NumberFormat.Format = "$#,##0.00";

            // Conditional formatting: red for low stock, yellow for near threshold
            if (product.StockQuantity <= 0)
                inventorySheet.Row(row).Style.Fill.BackgroundColor = XLColor.Red;
            else if (product.StockQuantity <= product.MinStockThreshold)
                inventorySheet.Row(row).Style.Fill.BackgroundColor = XLColor.Orange;
            else if (product.StockQuantity <= product.MinStockThreshold * 1.5)
                inventorySheet.Row(row).Style.Fill.BackgroundColor = XLColor.Yellow;

            row++;
        }

        inventorySheet.Range(1, 1, row - 1, 10).SetAutoFilter();
        inventorySheet.SheetView.FreezeRows(1);
        inventorySheet.Columns().AdjustToContents();

        // Low Stock sheet
        var lowStockSheet = workbook.Worksheets.Add("Low Stock");
        var lowHeaders = new[] { "SKU", "Name", "Category", "Stock", "Threshold", "Supplier", "Supplier Email", "Supplier Phone" };
        for (int i = 0; i < lowHeaders.Length; i++)
        {
            lowStockSheet.Cell(1, i + 1).Value = lowHeaders[i];
            lowStockSheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        row = 2;
        foreach (var product in products.Where(p => p.StockQuantity <= p.MinStockThreshold).OrderBy(p => p.StockQuantity))
        {
            lowStockSheet.Cell(row, 1).Value = product.SKU;
            lowStockSheet.Cell(row, 2).Value = product.Name;
            lowStockSheet.Cell(row, 3).Value = product.Category?.Name ?? "";
            lowStockSheet.Cell(row, 4).Value = product.StockQuantity;
            lowStockSheet.Cell(row, 5).Value = product.MinStockThreshold;
            lowStockSheet.Cell(row, 6).Value = product.Supplier?.Name ?? "";
            lowStockSheet.Cell(row, 7).Value = product.Supplier?.Email ?? "";
            lowStockSheet.Cell(row, 8).Value = product.Supplier?.Phone ?? "";
            lowStockSheet.Row(row).Style.Fill.BackgroundColor = XLColor.Red;
            row++;
        }

        lowStockSheet.SheetView.FreezeRows(1);
        lowStockSheet.Columns().AdjustToContents();

        return workbook.GetAsByteArray();
    }
}
