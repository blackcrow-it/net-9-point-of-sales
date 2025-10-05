using Domain.Entities.Sales;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Sales;

public class PaymentMethodTests
{
    [Fact]
    public void Validate_WhenCodeIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var paymentMethod = new PaymentMethod
        {
            Code = "",
            Name = "Cash",
            DisplayOrder = 1
        };

        // Act
        var act = () => paymentMethod.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code is required*");
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var paymentMethod = new PaymentMethod
        {
            Code = "CASH",
            Name = "",
            DisplayOrder = 1
        };

        // Act
        var act = () => paymentMethod.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WhenDisplayOrderIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var paymentMethod = new PaymentMethod
        {
            Code = "CASH",
            Name = "Cash",
            DisplayOrder = -1
        };

        // Act
        var act = () => paymentMethod.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Display order must be >= 0*");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var paymentMethod = new PaymentMethod
        {
            Code = "CASH",
            Name = "Cash",
            DisplayOrder = 1,
            Type = PaymentMethodType.Cash,
            IsActive = true
        };

        // Act
        var act = () => paymentMethod.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(PaymentMethodType.Cash)]
    [InlineData(PaymentMethodType.Card)]
    [InlineData(PaymentMethodType.EWallet)]
    [InlineData(PaymentMethodType.BankTransfer)]
    [InlineData(PaymentMethodType.Other)]
    public void PaymentMethodType_ShouldHaveAllExpectedValues(PaymentMethodType type)
    {
        // Assert
        type.Should().BeDefined();
    }
}
