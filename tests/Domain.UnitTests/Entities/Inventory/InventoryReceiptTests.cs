using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class InventoryReceiptTests
{
    [Fact]
    public void Complete_WhenDraft_ShouldSetStatusToCompleted()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            Status = ReceiptStatus.Draft,
            ReceiptNumber = "GRN-20250105-001"
        };

        // Act
        receipt.Complete();

        // Assert
        receipt.Status.Should().Be(ReceiptStatus.Completed);
    }

    [Fact]
    public void Complete_WhenNotDraft_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            Status = ReceiptStatus.Completed
        };

        // Act
        var act = () => receipt.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldSetStatusToCancelled()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            Status = ReceiptStatus.Draft
        };

        // Act
        receipt.Cancel();

        // Assert
        receipt.Status.Should().Be(ReceiptStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            Status = ReceiptStatus.Cancelled
        };

        // Act
        var act = () => receipt.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Receipt is already cancelled");
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            Status = ReceiptStatus.Completed
        };

        // Act
        var act = () => receipt.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel completed receipt");
    }

    [Fact]
    public void Validate_WhenReceiptNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            ReceiptNumber = "",
            TotalAmount = 1000m,
            ReceiptDate = DateTime.UtcNow
        };

        // Act
        var act = () => receipt.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Receipt number is required*");
    }

    [Fact]
    public void Validate_WhenTotalAmountIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            ReceiptNumber = "GRN-001",
            TotalAmount = -100m,
            ReceiptDate = DateTime.UtcNow
        };

        // Act
        var act = () => receipt.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Total amount must be >= 0*");
    }

    [Fact]
    public void Validate_WhenReceiptDateInFuture_ShouldThrowArgumentException()
    {
        // Arrange
        var receipt = new InventoryReceipt
        {
            ReceiptNumber = "GRN-001",
            TotalAmount = 1000m,
            ReceiptDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var act = () => receipt.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Receipt date cannot be in future*");
    }

    [Theory]
    [InlineData(ReceiptStatus.Draft)]
    [InlineData(ReceiptStatus.Completed)]
    [InlineData(ReceiptStatus.Cancelled)]
    public void ReceiptStatus_ShouldHaveAllExpectedValues(ReceiptStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
