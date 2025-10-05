using Domain.Entities.Sales;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Sales;

public class OrderTests
{
    [Fact]
    public void CalculateTotalAmount_ShouldCalculateCorrectly()
    {
        // Arrange
        var order = new Order
        {
            Subtotal = 100m,
            TaxAmount = 10m,
            DiscountAmount = 5m
        };

        // Act
        order.CalculateTotalAmount();

        // Assert
        order.TotalAmount.Should().Be(105m); // 100 + 10 - 5
    }

    [Fact]
    public void Validate_WhenOrderNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "",
            Subtotal = 100m
        };

        // Act
        var act = () => order.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Order number is required*");
    }

    [Fact]
    public void Validate_WhenSubtotalIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var order = new Order
        {
            OrderNumber = "ORD-20250105-0001",
            Subtotal = -10m
        };

        // Act
        var act = () => order.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Subtotal must be >= 0*");
    }

    [Fact]
    public void Complete_WhenOrderIsDraft_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Draft
        };

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAt.Should().NotBeNull();
        order.CompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_WhenOrderIsOnHold_ShouldChangeStatusToCompleted()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.OnHold
        };

        // Act
        order.Complete();

        // Assert
        order.Status.Should().Be(OrderStatus.Completed);
        order.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void Complete_WhenOrderIsAlreadyCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Completed
        };

        // Act
        var act = () => order.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot complete order with status*");
    }

    [Fact]
    public void Void_WithValidReason_ShouldChangeStatusToVoided()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Draft
        };
        var reason = "Customer cancelled";

        // Act
        order.Void(reason);

        // Assert
        order.Status.Should().Be(OrderStatus.Voided);
        order.VoidReason.Should().Be(reason);
        order.VoidedAt.Should().NotBeNull();
        order.VoidedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Void_WhenReasonIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Draft
        };

        // Act
        var act = () => order.Void("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Void reason is required*");
    }

    [Fact]
    public void Void_WhenOrderIsAlreadyVoided_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Voided
        };

        // Act
        var act = () => order.Void("Some reason");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order is already voided");
    }

    [Fact]
    public void Hold_WhenOrderIsDraft_ShouldChangeStatusToOnHold()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Draft
        };

        // Act
        order.Hold();

        // Assert
        order.Status.Should().Be(OrderStatus.OnHold);
    }

    [Fact]
    public void Hold_WhenOrderIsCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = new Order
        {
            Status = OrderStatus.Completed
        };

        // Act
        var act = () => order.Hold();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot hold order with status*");
    }

    [Fact]
    public void AddPayment_WithValidPayment_ShouldAddToPaymentsCollection()
    {
        // Arrange
        var order = new Order();
        var payment = new Payment
        {
            Amount = 100m,
            PaymentNumber = "PAY-20250105-0001"
        };

        // Act
        order.AddPayment(payment);

        // Assert
        order.Payments.Should().HaveCount(1);
        order.Payments.Should().Contain(payment);
    }

    [Fact]
    public void AddPayment_WhenPaymentIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var order = new Order();

        // Act
        var act = () => order.AddPayment(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(OrderStatus.Draft)]
    [InlineData(OrderStatus.Completed)]
    [InlineData(OrderStatus.Voided)]
    [InlineData(OrderStatus.Returned)]
    [InlineData(OrderStatus.OnHold)]
    public void OrderStatus_ShouldHaveAllExpectedValues(OrderStatus status)
    {
        // Assert - verify enum values are defined
        status.Should().BeDefined();
    }

    [Theory]
    [InlineData(OrderType.Sale)]
    [InlineData(OrderType.Return)]
    [InlineData(OrderType.Exchange)]
    public void OrderType_ShouldHaveAllExpectedValues(OrderType type)
    {
        // Assert - verify enum values are defined
        type.Should().BeDefined();
    }
}
