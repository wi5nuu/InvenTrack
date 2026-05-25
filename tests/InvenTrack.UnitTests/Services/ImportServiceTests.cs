using AutoMapper;
using InvenTrack.Application.DTOs.Import;
using InvenTrack.Application.Interfaces;
using InvenTrack.Application.Services;
using InvenTrack.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace InvenTrack.UnitTests.Services;

public class ImportServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ImportService>> _loggerMock;
    private readonly ImportService _importService;

    public ImportServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ImportService>>();
        _importService = new ImportService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task UploadAsync_WithNullFile_ReturnsFailure()
    {
        var result = await _importService.UploadAsync(null!, 1);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public async Task GetJobByIdAsync_WithNonExisting_ReturnsFailure()
    {
        _unitOfWorkMock.Setup(x => x.ImportJobs.GetByIdAsync(999)).ReturnsAsync((ImportJob?)null);
        var result = await _importService.GetJobByIdAsync(999);
        result.Success.Should().BeFalse();
    }

    [Fact]
    public void GenerateCsvTemplate_ReturnsNonEmptyBytes()
    {
        var bytes = _importService.GenerateCsvTemplate();
        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(50);
    }

    [Fact]
    public void GenerateExcelTemplate_ReturnsNonEmptyBytes()
    {
        var bytes = _importService.GenerateExcelTemplate();
        bytes.Should().NotBeEmpty();
        bytes.Length.Should().BeGreaterThan(1000);
    }
}
