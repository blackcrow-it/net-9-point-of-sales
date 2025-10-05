using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class InventoryIssueTests
{
    [Fact]
    public void Complete_WhenDraft_ShouldSetStatusToCompleted()
    {
        // Arrange
        var issue = new InventoryIssue
        {
            Status = IssueStatus.Draft
        };

        // Act
        issue.Complete();

        // Assert
        issue.Status.Should().Be(IssueStatus.Completed);
    }

    [Fact]
    public void Complete_WhenNotDraft_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var issue = new InventoryIssue
        {
            Status = IssueStatus.Completed
        };

        // Act
        var act = () => issue.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenDraft_ShouldSetStatusToCancelled()
    {
        // Arrange
        var issue = new InventoryIssue
        {
            Status = IssueStatus.Draft
        };

        // Act
        issue.Cancel();

        // Assert
        issue.Status.Should().Be(IssueStatus.Cancelled);
    }

    [Fact]
    public void Validate_WhenIssueNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var issue = new InventoryIssue
        {
            IssueNumber = "",
            Reason = "Damage"
        };

        // Act
        var act = () => issue.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Issue number is required*");
    }

    [Fact]
    public void Validate_WhenReasonIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var issue = new InventoryIssue
        {
            IssueNumber = "ISS-001",
            Reason = ""
        };

        // Act
        var act = () => issue.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Reason is required*");
    }

    [Fact]
    public void Validate_WhenTransferWithoutDestination_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var issue = new InventoryIssue
        {
            IssueNumber = "ISS-001",
            Reason = "Transfer",
            Type = IssueType.Transfer,
            DestinationStoreId = null
        };

        // Act
        var act = () => issue.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Destination store is required for transfers");
    }

    [Fact]
    public void Validate_WhenTransferToSameStore_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var issue = new InventoryIssue
        {
            IssueNumber = "ISS-001",
            Reason = "Transfer",
            Type = IssueType.Transfer,
            StoreId = storeId,
            DestinationStoreId = storeId
        };

        // Act
        var act = () => issue.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Destination store must differ from source store");
    }

    [Theory]
    [InlineData(IssueType.Adjustment)]
    [InlineData(IssueType.Damage)]
    [InlineData(IssueType.Loss)]
    [InlineData(IssueType.Transfer)]
    [InlineData(IssueType.Return)]
    public void IssueType_ShouldHaveAllExpectedValues(IssueType type)
    {
        // Assert
        type.Should().BeDefined();
    }

    [Theory]
    [InlineData(IssueStatus.Draft)]
    [InlineData(IssueStatus.Completed)]
    [InlineData(IssueStatus.Cancelled)]
    public void IssueStatus_ShouldHaveAllExpectedValues(IssueStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
