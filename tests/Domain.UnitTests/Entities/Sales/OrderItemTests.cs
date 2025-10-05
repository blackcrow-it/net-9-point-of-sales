using Domain.Entities.Sales;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Sales;

public class OrderItemTests
{
    [Fact]
    public void CalculateLineTotal_ShouldCalculateCorrectly()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 5m,
            UnitPrice = 100m,
            DiscountAmount = 50m,
            TaxRate = 10m // 10%
        };

        // Act
        orderItem.CalculateLineTotal();

        // Assert
        // Subtotal = (5 * 100 - 50) = 450
        // TaxAmount = 450 * 0.10 = 45
        // LineTotal = 450 + 45 = 495
        orderItem.TaxAmount.Should().Be(45m);
        orderItem.LineTotal.Should().Be(495m);
    }

    [Fact]
    public void CalculateLineTotal_WithZeroDiscount_ShouldCalculateCorrectly()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 2m,
            UnitPrice = 50m,
            DiscountAmount = 0m,
            TaxRate = 5m
        };

        // Act
        orderItem.CalculateLineTotal();

        // Assert
        // Subtotal = 100
        // TaxAmount = 100 * 0.05 = 5
        // LineTotal = 105
        orderItem.TaxAmount.Should().Be(5m);
        orderItem.LineTotal.Should().Be(105m);
    }

    [Fact]
    public void Validate_WhenQuantityIsZero_ShouldThrowArgumentException()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 0m,
            UnitPrice = 100m
        };

        // Act
        var act = () => orderItem.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be > 0*");
    }

    [Fact]
    public void Validate_WhenQuantityIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = -5m,
            UnitPrice = 100m
        };

        // Act
        var act = () => orderItem.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Quantity must be > 0*");
    }

    [Fact]
    public void Validate_WhenUnitPriceIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 5m,
            UnitPrice = -10m
        };

        // Act
        var act = () => orderItem.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Unit price must be >= 0*");
    }

    [Fact]
    public void Validate_WhenQuantityAndPriceAreValid_ShouldNotThrow()
    {
        // Arrange
        var orderItem = new OrderItem
        {
            Quantity = 5m,
            UnitPrice = 100m
        };

        // Act
        var act = () => orderItem.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
