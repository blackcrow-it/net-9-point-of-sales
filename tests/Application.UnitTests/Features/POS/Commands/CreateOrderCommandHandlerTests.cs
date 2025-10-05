using Application.Common.Interfaces;
using Application.Features.POS.Commands;
using Domain.Entities.Customers;
using Domain.Entities.Inventory;
using Domain.Entities.Sales;
using Domain.Entities.Store;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.POS.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateOrderCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidOrder_ShouldCreateSuccessfully()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();

        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.RetailStore };
        var customer = new Customer { Id = customerId, CustomerNumber = "CUS001", Name = "Test Customer" };
        var productVariant = new ProductVariant 
        { 
            Id = productVariantId, 
            SKU = "PRD001", 
            RetailPrice = 100000,
            ProductId = Guid.NewGuid()
        };

        var userId = Guid.NewGuid();
        var items = new List<OrderItemDto>
        {
            new(Guid.NewGuid(), productVariantId, 2, 100000, 0)
        };

        var command = new CreateOrderCommand(storeId, customerId, null, userId, items, "Test order");

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Customers).Returns(new List<Customer> { customer }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ProductVariants).Returns(new List<ProductVariant> { productVariant }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Orders).Returns(new List<Order>().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidStore_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateOrderCommand(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            null,
            Guid.NewGuid(),
            new List<OrderItemDto>(),
            null
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store>().BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Store not found");
    }

    [Fact]
    public async Task Handle_EmptyItems_ShouldReturnFailure()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.RetailStore };
        var customer = new Customer { Id = customerId, CustomerNumber = "CUS001", Name = "Test Customer" };

        var command = new CreateOrderCommand(
            storeId, 
            customerId, 
            null,
            Guid.NewGuid(),
            new List<OrderItemDto>(), // Empty items
            null
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Customers).Returns(new List<Customer> { customer }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Order must have at least one item");
    }
}
