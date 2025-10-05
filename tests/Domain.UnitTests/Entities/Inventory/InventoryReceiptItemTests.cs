using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class InventoryReceiptItemTests
{
    [Fact]
    public void CalculateLineTotal_ShouldCalculateCorrectly()
    {
        // Arrange
        var item = new InventoryReceiptItem
        {
            Quantity = 10m,
            UnitCost = 50m
        };

        // Act
        item.CalculateLineTotal();

        // Assert
        item.LineTotal.Should().Be(500m);
    }

    [Fact]
    public void Validate_WhenQuantityIsZero_ShouldThrowArgumentException()
    {
        // Arrange
        var item = new InventoryReceiptItem
        {
            Quantity = 0m,
            UnitCost = 50m
        };

        // Act
        var act = () => item.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be > 0*");
    }

    [Fact]
    public void Validate_WhenQuantityIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var item = new InventoryReceiptItem
        {
            Quantity = -5m,
            UnitCost = 50m
        };

        // Act
        var act = () => item.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be > 0*");
    }

    [Fact]
    public void Validate_WhenUnitCostIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var item = new InventoryReceiptItem
        {
            Quantity = 10m,
            UnitCost = -50m
        };

        // Act
        var act = () => item.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit cost must be >= 0*");
    }
}
