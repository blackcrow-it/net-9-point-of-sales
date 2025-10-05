using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class InventoryLevelTests
{
    [Fact]
    public void ReserveStock_WithValidQuantity_ShouldReduceAvailableAndIncreaseReserved()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 100m,
            ReservedQuantity = 10m,
            OnHandQuantity = 110m
        };

        // Act
        inventoryLevel.ReserveStock(20m);

        // Assert
        inventoryLevel.AvailableQuantity.Should().Be(80m);
        inventoryLevel.ReservedQuantity.Should().Be(30m);
    }

    [Fact]
    public void ReserveStock_WithInsufficientQuantity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 10m,
            ReservedQuantity = 5m
        };

        // Act
        var act = () => inventoryLevel.ReserveStock(20m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient available quantity");
    }

    [Fact]
    public void ReserveStock_WithZeroOrNegativeQuantity_ShouldThrowArgumentException()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 100m
        };

        // Act
        var act = () => inventoryLevel.ReserveStock(0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be > 0*");
    }

    [Fact]
    public void ReleaseStock_WithValidQuantity_ShouldIncreaseAvailableAndReduceReserved()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 80m,
            ReservedQuantity = 30m,
            OnHandQuantity = 110m
        };

        // Act
        inventoryLevel.ReleaseStock(15m);

        // Assert
        inventoryLevel.AvailableQuantity.Should().Be(95m);
        inventoryLevel.ReservedQuantity.Should().Be(15m);
    }

    [Fact]
    public void ReleaseStock_WithInsufficientReservedQuantity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 80m,
            ReservedQuantity = 10m
        };

        // Act
        var act = () => inventoryLevel.ReleaseStock(20m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient reserved quantity");
    }

    [Fact]
    public void AdjustQuantity_WithPositiveDelta_ShouldIncreaseAvailableAndOnHand()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 50m,
            ReservedQuantity = 10m,
            OnHandQuantity = 60m
        };

        // Act
        inventoryLevel.AdjustQuantity(25m);

        // Assert
        inventoryLevel.AvailableQuantity.Should().Be(75m);
        inventoryLevel.OnHandQuantity.Should().Be(85m); // 75 + 10
        inventoryLevel.ReservedQuantity.Should().Be(10m); // unchanged
    }

    [Fact]
    public void AdjustQuantity_WithNegativeDelta_ShouldDecreaseAvailableAndOnHand()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 50m,
            ReservedQuantity = 10m,
            OnHandQuantity = 60m
        };

        // Act
        inventoryLevel.AdjustQuantity(-20m);

        // Assert
        inventoryLevel.AvailableQuantity.Should().Be(30m);
        inventoryLevel.OnHandQuantity.Should().Be(40m); // 30 + 10
    }

    [Fact]
    public void AdjustQuantity_ResultingInNegativeAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 10m,
            ReservedQuantity = 5m,
            OnHandQuantity = 15m
        };

        // Act
        var act = () => inventoryLevel.AdjustQuantity(-15m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Available quantity cannot be negative");
    }

    [Fact]
    public void Validate_WhenOnHandEqualsAvailablePlusReserved_ShouldNotThrow()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 50m,
            ReservedQuantity = 25m,
            OnHandQuantity = 75m
        };

        // Act
        var act = () => inventoryLevel.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenOnHandDoesNotEqualAvailablePlusReserved_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 50m,
            ReservedQuantity = 25m,
            OnHandQuantity = 100m // Should be 75
        };

        // Act
        var act = () => inventoryLevel.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("OnHandQuantity must equal AvailableQuantity + ReservedQuantity");
    }

    [Fact]
    public void CompleteWorkflow_ReserveReleaseAdjust_ShouldMaintainDataIntegrity()
    {
        // Arrange - Start with 100 units
        var inventoryLevel = new InventoryLevel
        {
            AvailableQuantity = 100m,
            ReservedQuantity = 0m,
            OnHandQuantity = 100m
        };

        // Act & Assert - Reserve 30 units for order
        inventoryLevel.ReserveStock(30m);
        inventoryLevel.AvailableQuantity.Should().Be(70m);
        inventoryLevel.ReservedQuantity.Should().Be(30m);

        // Reserve another 20 units
        inventoryLevel.ReserveStock(20m);
        inventoryLevel.AvailableQuantity.Should().Be(50m);
        inventoryLevel.ReservedQuantity.Should().Be(50m);

        // Release 30 units (first order cancelled)
        inventoryLevel.ReleaseStock(30m);
        inventoryLevel.AvailableQuantity.Should().Be(80m);
        inventoryLevel.ReservedQuantity.Should().Be(20m);

        // Receive 50 new units
        inventoryLevel.AdjustQuantity(50m);
        inventoryLevel.AvailableQuantity.Should().Be(130m);
        inventoryLevel.OnHandQuantity.Should().Be(150m); // 130 + 20

        // Validate final state
        inventoryLevel.Validate();
    }
}
