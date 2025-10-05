using Domain.Entities.Customers;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Customers;

public class DebtTests
{
    [Fact]
    public void RecordPayment_WithValidAmount_ShouldUpdatePaidAndRemaining()
    {
        // Arrange
        var debt = new Debt
        {
            Amount = 1000m,
            PaidAmount = 300m,
            RemainingAmount = 700m,
            Status = DebtStatus.PartiallyPaid
        };

        // Act
        debt.RecordPayment(400m);

        // Assert
        debt.PaidAmount.Should().Be(700m);
        debt.RemainingAmount.Should().Be(300m);
        debt.Status.Should().Be(DebtStatus.PartiallyPaid);
    }

    [Fact]
    public void RecordPayment_WhenFullyPaid_ShouldSetStatusToPaid()
    {
        // Arrange
        var debt = new Debt
        {
            Amount = 1000m,
            PaidAmount = 700m,
            RemainingAmount = 300m,
            Status = DebtStatus.PartiallyPaid
        };

        // Act
        debt.RecordPayment(300m);

        // Assert
        debt.PaidAmount.Should().Be(1000m);
        debt.RemainingAmount.Should().Be(0m);
        debt.Status.Should().Be(DebtStatus.Paid);
        debt.PaidDate.Should().NotBeNull();
        debt.PaidDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RecordPayment_WhenFirstPayment_ShouldSetStatusToPartiallyPaid()
    {
        // Arrange
        var debt = new Debt
        {
            Amount = 1000m,
            PaidAmount = 0m,
            RemainingAmount = 1000m,
            Status = DebtStatus.Pending
        };

        // Act
        debt.RecordPayment(200m);

        // Assert
        debt.Status.Should().Be(DebtStatus.PartiallyPaid);
    }

    [Fact]
    public void RecordPayment_WithZeroOrNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var debt = new Debt
        {
            Amount = 1000m,
            RemainingAmount = 1000m
        };

        // Act
        var act = () => debt.RecordPayment(0m);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount must be > 0*");
    }

    [Fact]
    public void RecordPayment_WhenExceedsRemaining_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var debt = new Debt
        {
            Amount = 1000m,
            PaidAmount = 700m,
            RemainingAmount = 300m
        };

        // Act
        var act = () => debt.RecordPayment(500m);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Payment amount exceeds remaining debt");
    }

    [Fact]
    public void MarkOverdue_WhenPending_ShouldSetStatusToOverdue()
    {
        // Arrange
        var debt = new Debt
        {
            Status = DebtStatus.Pending
        };

        // Act
        debt.MarkOverdue();

        // Assert
        debt.Status.Should().Be(DebtStatus.Overdue);
    }

    [Fact]
    public void MarkOverdue_WhenPaid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var debt = new Debt
        {
            Status = DebtStatus.Paid
        };

        // Act
        var act = () => debt.MarkOverdue();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void WriteOff_WhenNotPaid_ShouldSetStatusToWrittenOff()
    {
        // Arrange
        var debt = new Debt
        {
            Status = DebtStatus.Overdue
        };

        // Act
        debt.WriteOff();

        // Assert
        debt.Status.Should().Be(DebtStatus.WrittenOff);
    }

    [Fact]
    public void WriteOff_WhenPaid_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var debt = new Debt
        {
            Status = DebtStatus.Paid
        };

        // Act
        var act = () => debt.WriteOff();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot write off paid debt");
    }

    [Fact]
    public void CalculateRemainingAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var debt = new Debt
        {
            Amount = 1000m,
            PaidAmount = 350m
        };

        // Act
        debt.CalculateRemainingAmount();

        // Assert
        debt.RemainingAmount.Should().Be(650m);
    }

    [Fact]
    public void Validate_WhenDebtNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var debt = new Debt
        {
            DebtNumber = "",
            Amount = 1000m,
            RemainingAmount = 1000m
        };

        // Act
        var act = () => debt.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Debt number is required*");
    }

    [Fact]
    public void Validate_WhenAmountIsZeroOrNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var debt = new Debt
        {
            DebtNumber = "DBT-001",
            Amount = 0m,
            RemainingAmount = 0m
        };

        // Act
        var act = () => debt.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount must be > 0*");
    }

    [Fact]
    public void Validate_WhenPaidWithoutDate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var debt = new Debt
        {
            DebtNumber = "DBT-001",
            Amount = 1000m,
            PaidAmount = 1000m,
            RemainingAmount = 0m,
            Status = DebtStatus.Paid,
            PaidDate = null
        };

        // Act
        var act = () => debt.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Paid date is required when status is Paid");
    }

    [Fact]
    public void Validate_WhenRemainingDoesNotMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var debt = new Debt
        {
            DebtNumber = "DBT-001",
            Amount = 1000m,
            PaidAmount = 300m,
            RemainingAmount = 800m // Should be 700
        };

        // Act
        var act = () => debt.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("RemainingAmount must equal Amount - PaidAmount");
    }

    [Theory]
    [InlineData(DebtStatus.Pending)]
    [InlineData(DebtStatus.PartiallyPaid)]
    [InlineData(DebtStatus.Paid)]
    [InlineData(DebtStatus.Overdue)]
    [InlineData(DebtStatus.WrittenOff)]
    public void DebtStatus_ShouldHaveAllExpectedValues(DebtStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
