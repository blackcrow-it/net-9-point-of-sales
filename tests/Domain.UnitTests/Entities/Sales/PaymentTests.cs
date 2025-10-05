using Domain.Entities.Sales;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Sales;

public class PaymentTests
{
    [Fact]
    public void MarkAsCompleted_WhenPending_ShouldSetStatusToCompleted()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Pending,
            Amount = 100m
        };

        // Act
        payment.MarkAsCompleted("user123");

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.ProcessedAt.Should().NotBeNull();
        payment.ProcessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        payment.ProcessedBy.Should().Be("user123");
    }

    [Fact]
    public void MarkAsCompleted_WhenAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Completed
        };

        // Act
        var act = () => payment.MarkAsCompleted();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Payment is already completed");
    }

    [Fact]
    public void MarkAsFailed_WhenPending_ShouldSetStatusToFailed()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Pending
        };

        // Act
        payment.MarkAsFailed();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public void MarkAsFailed_WhenAlreadyFailed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Failed
        };

        // Act
        var act = () => payment.MarkAsFailed();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsFailed_WhenRefunded_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Refunded
        };

        // Act
        var act = () => payment.MarkAsFailed();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Refund_WhenCompleted_ShouldSetStatusToRefunded()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Completed
        };

        // Act
        payment.Refund();

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
    }

    [Fact]
    public void Refund_WhenNotCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var payment = new Payment
        {
            Status = PaymentStatus.Pending
        };

        // Act
        var act = () => payment.Refund();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Only completed payments can be refunded");
    }

    [Fact]
    public void Validate_WhenAmountIsZero_ShouldThrowArgumentException()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = 0m
        };

        // Act
        var act = () => payment.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount must be > 0*");
    }

    [Fact]
    public void Validate_WhenAmountIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var payment = new Payment
        {
            Amount = -100m
        };

        // Act
        var act = () => payment.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Amount must be > 0*");
    }

    [Theory]
    [InlineData(PaymentStatus.Pending)]
    [InlineData(PaymentStatus.Completed)]
    [InlineData(PaymentStatus.Failed)]
    [InlineData(PaymentStatus.Refunded)]
    public void PaymentStatus_ShouldHaveAllExpectedValues(PaymentStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
