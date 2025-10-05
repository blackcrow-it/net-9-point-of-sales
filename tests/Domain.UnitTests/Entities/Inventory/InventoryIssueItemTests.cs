using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class InventoryIssueItemTests
{
    [Fact]
    public void Validate_WhenQuantityIsZero_ShouldThrowArgumentException()
    {
        // Arrange
        var item = new InventoryIssueItem
        {
            Quantity = 0m
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
        var item = new InventoryIssueItem
        {
            Quantity = -10m
        };

        // Act
        var act = () => item.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be > 0*");
    }

    [Fact]
    public void Validate_WhenQuantityIsPositive_ShouldNotThrow()
    {
        // Arrange
        var item = new InventoryIssueItem
        {
            Quantity = 10m
        };

        // Act
        var act = () => item.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
