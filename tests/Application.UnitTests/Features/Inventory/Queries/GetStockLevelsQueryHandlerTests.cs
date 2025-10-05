using Application.Common.Interfaces;
using Application.Features.Inventory.Queries;
using Domain.Entities.Inventory;
using Domain.Entities.Store;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Inventory.Queries;

public class GetStockLevelsQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetStockLevelsQueryHandler _handler;

    public GetStockLevelsQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetStockLevelsQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnStockLevels()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();

        var product = new Product
        {
            Id = productId,
            SKU = "TEST-001",
            Name = "Test Product",
            Description = "Test",
            IsActive = true
        };

        var variant = new ProductVariant
        {
            Id = variantId,
            ProductId = productId,
            Name = "Default",
            SKU = "TEST-001-DEFAULT",
            IsActive = true,
            Product = product
        };

        var store = new Store
        {
            Id = storeId,
            Code = "ST001",
            Name = "Test Store",
            IsActive = true
        };

        var levels = new List<InventoryLevel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                ProductVariantId = variantId,
                OnHandQuantity = 100,
                AvailableQuantity = 90,
                ReservedQuantity = 10,
                Store = store,
                ProductVariant = variant
            }
        };

        var mockSet = levels.BuildMockDbSet();
        _contextMock.Setup(c => c.InventoryLevels).Returns(mockSet.Object);

        var query = new GetStockLevelsQuery(
            StoreId: storeId,
            ProductId: null,
            LowStock: null,
            PageNumber: 1,
            PageSize: 50
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items[0].OnHandQuantity.Should().Be(100);
        result.Data.Items[0].AvailableQuantity.Should().Be(90);
    }

    [Fact]
    public async Task Handle_LowStockFilter_ShouldReturnOnlyLowStockItems()
    {
        // Arrange
        var storeId = Guid.NewGuid();

        var store = new Store
        {
            Id = storeId,
            Code = "ST001",
            Name = "Test Store",
            IsActive = true
        };

        var product1 = new Product
        {
            Id = Guid.NewGuid(),
            SKU = "TEST-001",
            Name = "Low Stock Product",
            IsActive = true
        };

        var product2 = new Product
        {
            Id = Guid.NewGuid(),
            SKU = "TEST-002",
            Name = "Good Stock Product",
            IsActive = true
        };

        var variant1 = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product1.Id,
            Name = "Default",
            SKU = "TEST-001-DEFAULT",
            IsActive = true,
            Product = product1
        };

        var variant2 = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product2.Id,
            Name = "Default",
            SKU = "TEST-002-DEFAULT",
            IsActive = true,
            Product = product2
        };

        var levels = new List<InventoryLevel>
        {
            new()
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                ProductVariantId = variant1.Id,
                OnHandQuantity = 5,
                AvailableQuantity = 5,
                ReservedQuantity = 0,
                Store = store,
                ProductVariant = variant1
            },
            new()
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                ProductVariantId = variant2.Id,
                OnHandQuantity = 100,
                AvailableQuantity = 100,
                ReservedQuantity = 0,
                Store = store,
                ProductVariant = variant2
            }
        };

        var mockSet = levels.BuildMockDbSet();
        _contextMock.Setup(c => c.InventoryLevels).Returns(mockSet.Object);

        var query = new GetStockLevelsQuery(
            StoreId: storeId,
            ProductId: null,
            LowStock: true,
            PageNumber: 1,
            PageSize: 50
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1);
        result.Data.Items[0].ProductName.Should().Be("Low Stock Product");
    }
}
