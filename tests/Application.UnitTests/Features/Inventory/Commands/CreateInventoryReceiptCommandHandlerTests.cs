using Application.Common.Interfaces;
using Application.Features.Inventory.Commands;
using Domain.Entities.Inventory;
using Domain.Entities.Store;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Inventory.Commands;

public class CreateInventoryReceiptCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateInventoryReceiptCommandHandler _handler;

    public CreateInventoryReceiptCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateInventoryReceiptCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateReceipt()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var productVariantId = Guid.NewGuid();

        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.Warehouse };
        var supplier = new Supplier { Id = supplierId, Code = "SUP001", Name = "Main Supplier" };
        var variant = new ProductVariant { Id = productVariantId, SKU = "PRD001", ProductId = Guid.NewGuid() };

        var items = new List<ReceiptItemDto>
        {
            new(Guid.NewGuid(), productVariantId, 100, 50000)
        };

        var command = new CreateInventoryReceiptCommand(
            storeId,
            supplierId,
            "GRN-20251005-0001",
            DateTime.UtcNow,
            items,
            null
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Suppliers).Returns(new List<Supplier> { supplier }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.ProductVariants).Returns(new List<ProductVariant> { variant }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.InventoryReceipts).Returns(new List<InventoryReceipt>().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.InventoryReceiptItems).Returns(new List<InventoryReceiptItem>().BuildMockDbSet().Object);
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
        var command = new CreateInventoryReceiptCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "GRN-20251005-0001",
            DateTime.UtcNow,
            new List<ReceiptItemDto>(),
            null
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store>().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.InventoryReceipts).Returns(new List<InventoryReceipt>().BuildMockDbSet().Object);

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
        var supplierId = Guid.NewGuid();
        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.Warehouse };
        var supplier = new Supplier { Id = supplierId, Code = "SUP001", Name = "Test Supplier" };

        var command = new CreateInventoryReceiptCommand(
            storeId,
            supplierId,
            "GRN-20251005-0001",
            DateTime.UtcNow,
            new List<ReceiptItemDto>(), // Empty
            null
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Suppliers).Returns(new List<Supplier> { supplier }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.InventoryReceipts).Returns(new List<InventoryReceipt>().BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("at least one item"));
    }
}
