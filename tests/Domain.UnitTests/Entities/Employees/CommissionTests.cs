using Domain.Entities.Employees;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Employees;

public class CommissionTests
{
    [Fact]
    public void CalculateCommissionAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var commission = new Commission
        {
            OrderAmount = 1000m,
            CommissionRate = 5m // 5%
        };

        // Act
        commission.CalculateCommissionAmount();

        // Assert
        commission.CommissionAmount.Should().Be(50m); // 1000 * 0.05
    }

    [Fact]
    public void Validate_WhenAmountDoesNotMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var commission = new Commission
        {
            OrderAmount = 1000m,
            CommissionRate = 5m,
            CommissionAmount = 100m // Should be 50
        };

        // Act
        var act = () => commission.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CommissionAmount must equal OrderAmount * (CommissionRate / 100)");
    }

    [Fact]
    public void Validate_WhenPaidWithoutDate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var commission = new Commission
        {
            OrderAmount = 1000m,
            CommissionRate = 5m,
            CommissionAmount = 50m,
            Status = CommissionStatus.Paid,
            PaidDate = null
        };

        // Act
        var act = () => commission.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Paid date is required when status is Paid");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var commission = new Commission
        {
            OrderAmount = 1000m,
            CommissionRate = 5m,
            CommissionAmount = 50m,
            Status = CommissionStatus.Pending
        };

        // Act
        var act = () => commission.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(CommissionStatus.Pending)]
    [InlineData(CommissionStatus.Approved)]
    [InlineData(CommissionStatus.Paid)]
    [InlineData(CommissionStatus.Cancelled)]
    public void CommissionStatus_ShouldHaveAllExpectedValues(CommissionStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
