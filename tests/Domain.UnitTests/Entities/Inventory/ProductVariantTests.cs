using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class ProductVariantTests
{
    [Fact]
    public void Validate_WhenSKUIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var variant = new ProductVariant
        {
            SKU = "",
            ProductId = Guid.NewGuid()
        };

        // Act
        var act = () => variant.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("SKU is required*");
    }

    [Fact]
    public void Validate_WithValidSKU_ShouldNotThrow()
    {
        // Arrange
        var variant = new ProductVariant
        {
            SKU = "VAR-001",
            ProductId = Guid.NewGuid()
        };

        // Act
        var act = () => variant.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
