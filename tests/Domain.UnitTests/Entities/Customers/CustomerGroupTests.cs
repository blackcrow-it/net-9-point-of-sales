using Domain.Entities.Customers;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Customers;

public class CustomerGroupTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var group = new CustomerGroup
        {
            Name = "",
            DiscountPercentage = 10m,
            LoyaltyPointsMultiplier = 1.5m
        };

        // Act
        var act = () => group.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_WhenDiscountPercentageOutOfRange_ShouldThrowArgumentException(decimal percentage)
    {
        // Arrange
        var group = new CustomerGroup
        {
            Name = "VIP",
            DiscountPercentage = percentage,
            LoyaltyPointsMultiplier = 1.5m
        };

        // Act
        var act = () => group.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Discount percentage must be between 0 and 100*");
    }

    [Fact]
    public void Validate_WhenLoyaltyMultiplierIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var group = new CustomerGroup
        {
            Name = "VIP",
            DiscountPercentage = 10m,
            LoyaltyPointsMultiplier = -1m
        };

        // Act
        var act = () => group.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Loyalty points multiplier must be >= 0*");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var group = new CustomerGroup
        {
            Name = "VIP",
            DiscountPercentage = 15m,
            LoyaltyPointsMultiplier = 2.0m,
            IsActive = true
        };

        // Act
        var act = () => group.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
