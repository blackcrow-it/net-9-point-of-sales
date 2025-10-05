using Domain.Entities.Sales;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Sales;

public class ShiftTests
{
    [Fact]
    public void CloseShift_WhenOpen_ShouldSetStatusToClosedAndCalculateDifference()
    {
        // Arrange
        var shift = new Shift
        {
            Status = ShiftStatus.Open,
            OpeningCash = 500m
        };

        // Act
        shift.CloseShift(closingCash: 1500m, expectedCash: 1450m);

        // Assert
        shift.Status.Should().Be(ShiftStatus.Closed);
        shift.EndTime.Should().NotBeNull();
        shift.EndTime.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        shift.ClosingCash.Should().Be(1500m);
        shift.ExpectedCash.Should().Be(1450m);
        shift.CashDifference.Should().Be(50m); // Over by 50
    }

    [Fact]
    public void CloseShift_WhenAlreadyClosed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shift = new Shift
        {
            Status = ShiftStatus.Closed
        };

        // Act
        var act = () => shift.CloseShift(1000m, 1000m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Shift is already closed");
    }

    [Fact]
    public void CalculateCashDifference_ShouldCalculateCorrectly()
    {
        // Arrange
        var shift = new Shift
        {
            ClosingCash = 1200m,
            ExpectedCash = 1250m
        };

        // Act
        shift.CalculateCashDifference();

        // Assert
        shift.CashDifference.Should().Be(-50m); // Short by 50
    }

    [Fact]
    public void Validate_WhenOpeningCashIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var shift = new Shift
        {
            OpeningCash = -100m
        };

        // Act
        var act = () => shift.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Opening cash must be >= 0*");
    }

    [Fact]
    public void Validate_WhenClosedButNoEndTime_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var shift = new Shift
        {
            Status = ShiftStatus.Closed,
            OpeningCash = 500m,
            EndTime = null
        };

        // Act
        var act = () => shift.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("End time is required when shift is closed");
    }

    [Fact]
    public void Validate_WhenOpenWithValidData_ShouldNotThrow()
    {
        // Arrange
        var shift = new Shift
        {
            Status = ShiftStatus.Open,
            OpeningCash = 500m
        };

        // Act
        var act = () => shift.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(ShiftStatus.Open)]
    [InlineData(ShiftStatus.Closed)]
    public void ShiftStatus_ShouldHaveAllExpectedValues(ShiftStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
