using Domain.Entities.Customers;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Customers;

public class LoyaltyTransactionTests
{
    [Fact]
    public void Validate_WhenEarnedWithNegativePoints_ShouldThrowArgumentException()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Earned,
            Points = -10m
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Points must be > 0 for Earned transactions*");
    }

    [Fact]
    public void Validate_WhenRedeemedWithPositivePoints_ShouldThrowArgumentException()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Redeemed,
            Points = 10m,
            OrderId = Guid.NewGuid()
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Points must be < 0 for Redeemed/Expired transactions*");
    }

    [Fact]
    public void Validate_WhenExpiredWithPositivePoints_ShouldThrowArgumentException()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Expired,
            Points = 5m
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Points must be < 0 for Redeemed/Expired transactions*");
    }

    [Fact]
    public void Validate_WhenEarnedWithoutOrderId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Earned,
            Points = 50m,
            OrderId = null
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order ID is required for Earned/Redeemed transactions");
    }

    [Fact]
    public void Validate_WhenRedeemedWithoutOrderId_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Redeemed,
            Points = -30m,
            OrderId = null
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order ID is required for Earned/Redeemed transactions");
    }

    [Fact]
    public void Validate_WhenEarnedWithValidData_ShouldNotThrow()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Earned,
            Points = 100m,
            OrderId = Guid.NewGuid()
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenRedeemedWithValidData_ShouldNotThrow()
    {
        // Arrange
        var transaction = new LoyaltyTransaction
        {
            Type = LoyaltyTransactionType.Redeemed,
            Points = -50m,
            OrderId = Guid.NewGuid()
        };

        // Act
        var act = () => transaction.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(LoyaltyTransactionType.Earned)]
    [InlineData(LoyaltyTransactionType.Redeemed)]
    [InlineData(LoyaltyTransactionType.Adjusted)]
    [InlineData(LoyaltyTransactionType.Expired)]
    public void LoyaltyTransactionType_ShouldHaveAllExpectedValues(LoyaltyTransactionType type)
    {
        // Assert
        type.Should().BeDefined();
    }
}
