using Domain.Entities.Customers;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Customers;

public class CustomerTests
{
    [Fact]
    public void AddLoyaltyPoints_WithValidPoints_ShouldIncreaseBalance()
    {
        // Arrange
        var customer = new Customer
        {
            LoyaltyPoints = 100m
        };

        // Act
        customer.AddLoyaltyPoints(50m);

        // Assert
        customer.LoyaltyPoints.Should().Be(150m);
    }

    [Fact]
    public void AddLoyaltyPoints_WithZeroOrNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer();

        // Act
        var act = () => customer.AddLoyaltyPoints(0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Points must be > 0*");
    }

    [Fact]
    public void RedeemPoints_WithSufficientBalance_ShouldDecreaseBalance()
    {
        // Arrange
        var customer = new Customer
        {
            LoyaltyPoints = 100m
        };

        // Act
        customer.RedeemPoints(30m);

        // Assert
        customer.LoyaltyPoints.Should().Be(70m);
    }

    [Fact]
    public void RedeemPoints_WithInsufficientBalance_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = new Customer
        {
            LoyaltyPoints = 50m
        };

        // Act
        var act = () => customer.RedeemPoints(100m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Insufficient loyalty points");
    }

    [Fact]
    public void RecordPurchase_ShouldUpdateTotalSpentOrdersAndDate()
    {
        // Arrange
        var customer = new Customer
        {
            TotalSpent = 1000m,
            TotalOrders = 5
        };

        // Act
        customer.RecordPurchase(250m);

        // Assert
        customer.TotalSpent.Should().Be(1250m);
        customer.TotalOrders.Should().Be(6);
        customer.LastPurchaseDate.Should().NotBeNull();
        customer.LastPurchaseDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordPurchase_WithZeroOrNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer();

        // Act
        var act = () => customer.RecordPurchase(0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount must be > 0*");
    }

    [Fact]
    public void Validate_WhenCustomerNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerNumber = "",
            Phone = "0123456789"
        };

        // Act
        var act = () => customer.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Customer number is required*");
    }

    [Fact]
    public void Validate_WhenBothPhoneAndEmailEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerNumber = "CUS-001",
            Phone = null,
            Email = null
        };

        // Act
        var act = () => customer.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Either phone or email is required");
    }

    [Fact]
    public void Validate_WhenPhoneIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerNumber = "CUS-001",
            Phone = "123" // Invalid: not 10 digits
        };

        // Act
        var act = () => customer.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Phone must be 10 digits (Vietnamese format)*");
    }

    [Fact]
    public void Validate_WhenEmailIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerNumber = "CUS-001",
            Email = "invalid-email"
        };

        // Act
        var act = () => customer.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email must be valid format*");
    }

    [Fact]
    public void Validate_WithValidPhone_ShouldNotThrow()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerNumber = "CUS-001",
            Phone = "0987654321" // Valid 10-digit Vietnamese phone
        };

        // Act
        var act = () => customer.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidEmail_ShouldNotThrow()
    {
        // Arrange
        var customer = new Customer
        {
            CustomerNumber = "CUS-001",
            Email = "customer@example.com"
        };

        // Act
        var act = () => customer.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(CustomerGender.Male)]
    [InlineData(CustomerGender.Female)]
    [InlineData(CustomerGender.Other)]
    public void CustomerGender_ShouldHaveAllExpectedValues(CustomerGender gender)
    {
        // Assert
        gender.Should().BeDefined();
    }
}
