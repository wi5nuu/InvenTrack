using AutoMapper;
using InvenTrack.Application.DTOs.Products;
using InvenTrack.Application.Interfaces;
using InvenTrack.Application.Services;
using InvenTrack.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace InvenTrack.UnitTests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _loggerMock = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_unitOfWorkMock.Object, _mapperMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidData_ReturnsSuccess()
    {
        var dto = new CreateProductDto { SKU = "TEST-001", Name = "Test Product", CategoryId = 1, UnitPrice = 100, CostPrice = 50, StockQuantity = 10, MinStockThreshold = 2 };
        var category = new Category { Id = 1, Name = "Test Category" };

        _unitOfWorkMock.Setup(x => x.Products.GetBySkuAsync("TEST-001")).ReturnsAsync((Product?)null);
        _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(1)).ReturnsAsync(category);
        _unitOfWorkMock.Setup(x => x.Products.AddAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.StockMovements.AddAsync(It.IsAny<StockMovement>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<Product>(It.IsAny<CreateProductDto>())).Returns(new Product { SKU = "TEST-001", Name = "Test Product" });
        _mapperMock.Setup(x => x.Map<ProductDto>(It.IsAny<Product>())).Returns(new ProductDto { SKU = "TEST-001", Name = "Test Product" });

        var result = await _productService.CreateAsync(dto, 1);

        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSku_ReturnsFailure()
    {
        var dto = new CreateProductDto { SKU = "EXIST-001", Name = "Test", CategoryId = 1, UnitPrice = 100, CostPrice = 50 };
        _unitOfWorkMock.Setup(x => x.Products.GetBySkuAsync("EXIST-001")).ReturnsAsync(new Product { SKU = "EXIST-001" });

        var result = await _productService.CreateAsync(dto, 1);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already exists");
    }

    [Fact]
    public async Task GetProductById_WithNonExistingId_ReturnsFailure()
    {
        _unitOfWorkMock.Setup(x => x.Products.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var result = await _productService.GetByIdAsync(999);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task AdjustStock_UpdatesQuantityAndCreatesMovement()
    {
        var product = new Product { Id = 1, SKU = "ADJ-001", Name = "Adjustable", StockQuantity = 50, MinStockThreshold = 5 };
        var dto = new AdjustStockDto { Quantity = 30, Notes = "Manual adjustment" };

        _unitOfWorkMock.Setup(x => x.Products.GetByIdAsync(1)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _mapperMock.Setup(x => x.Map<ProductDto>(It.IsAny<Product>())).Returns(new ProductDto { SKU = "ADJ-001", StockQuantity = 30 });

        var result = await _productService.AdjustStockAsync(1, dto, 1);

        result.Success.Should().BeTrue();
        product.StockQuantity.Should().Be(30);
    }
}
