using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class ProductTests
{
    [Fact]
    public void Validate_WhenSKUIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var product = new Product
        {
            SKU = "",
            Name = "Test Product"
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("SKU is required*");
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var product = new Product
        {
            SKU = "PROD-001",
            Name = ""
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WhenCostPriceIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var product = new Product
        {
            SKU = "PROD-001",
            Name = "Test Product",
            CostPrice = -10m
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Cost price must be >= 0*");
    }

    [Fact]
    public void Validate_WhenRetailPriceIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var product = new Product
        {
            SKU = "PROD-001",
            Name = "Test Product",
            RetailPrice = -20m
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Retail price must be >= 0*");
    }

    [Fact]
    public void Validate_WhenTrackInventoryTrueButNoReorderLevel_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var product = new Product
        {
            SKU = "PROD-001",
            Name = "Test Product",
            TrackInventory = true,
            ReorderLevel = null,
            ReorderQuantity = 100m
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Reorder level and quantity are required when tracking inventory");
    }

    [Fact]
    public void Validate_WhenTrackInventoryTrueButNoReorderQuantity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var product = new Product
        {
            SKU = "PROD-001",
            Name = "Test Product",
            TrackInventory = true,
            ReorderLevel = 10m,
            ReorderQuantity = null
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Reorder level and quantity are required when tracking inventory");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var product = new Product
        {
            SKU = "PROD-001",
            Name = "Test Product",
            CostPrice = 50m,
            RetailPrice = 100m,
            TrackInventory = true,
            ReorderLevel = 10m,
            ReorderQuantity = 100m
        };

        // Act
        var act = () => product.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(ProductType.Single)]
    [InlineData(ProductType.Variable)]
    public void ProductType_ShouldHaveAllExpectedValues(ProductType type)
    {
        // Assert
        type.Should().BeDefined();
    }
}
