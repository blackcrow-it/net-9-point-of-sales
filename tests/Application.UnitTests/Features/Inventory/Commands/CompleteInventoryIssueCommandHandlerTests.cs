using Application.Common.Interfaces;
using Application.Features.Inventory.Commands;
using Domain.Entities.Inventory;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Inventory.Commands;

public class CompleteInventoryIssueCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CompleteInventoryIssueCommandHandler _handler;

    public CompleteInventoryIssueCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CompleteInventoryIssueCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidAdjustment_ShouldCompleteAndUpdateInventory()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var issueId = Guid.NewGuid();

        var issue = new InventoryIssue
        {
            Id = issueId,
            IssueNumber = "ISS-20250105-0001",
            StoreId = storeId,
            Type = Domain.Entities.Inventory.IssueType.Adjustment,
            Status = Domain.Entities.Inventory.IssueStatus.Draft,
            IssueDate = DateTime.UtcNow,
            Reason = "Test adjustment",
            Items = new List<InventoryIssueItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    IssueId = issueId,
                    ProductVariantId = variantId,
                    Quantity = 5,
                    LineNumber = 1,
                    Unit = "pcs"
                }
            }
        };

        var inventoryLevel = new InventoryLevel
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ProductVariantId = variantId,
            OnHandQuantity = 100,
            AvailableQuantity = 100,
            ReservedQuantity = 0
        };

        var issues = new List<InventoryIssue> { issue };
        var levels = new List<InventoryLevel> { inventoryLevel };

        var issuesMock = issues.BuildMockDbSet();
        issuesMock.Setup(x => x.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((object[] ids, CancellationToken _) =>
                issues.FirstOrDefault(i => i.Id == (Guid)ids[0]));

        var levelsMock = levels.BuildMockDbSet();

        _contextMock.Setup(c => c.InventoryIssues).Returns(issuesMock.Object);
        _contextMock.Setup(c => c.InventoryLevels).Returns(levelsMock.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CompleteInventoryIssueCommand(issueId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(issueId);
        issue.Status.Should().Be(Domain.Entities.Inventory.IssueStatus.Completed);
        inventoryLevel.OnHandQuantity.Should().Be(95); // 100 - 5
        inventoryLevel.AvailableQuantity.Should().Be(95);
    }

    [Fact]
    public async Task Handle_IssueNotFound_ShouldReturnFailure()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var issues = new List<InventoryIssue>();

        var issuesMock = issues.BuildMockDbSet();
        _contextMock.Setup(c => c.InventoryIssues).Returns(issuesMock.Object);

        var command = new CompleteInventoryIssueCommand(issueId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    [Fact]
    public async Task Handle_AlreadyCompleted_ShouldReturnFailure()
    {
        // Arrange
        var issueId = Guid.NewGuid();
        var issue = new InventoryIssue
        {
            Id = issueId,
            IssueNumber = "ISS-20250105-0001",
            StoreId = Guid.NewGuid(),
            Type = Domain.Entities.Inventory.IssueType.Adjustment,
            Status = Domain.Entities.Inventory.IssueStatus.Completed, // Already completed
            IssueDate = DateTime.UtcNow,
            Reason = "Test",
            Items = new List<InventoryIssueItem>()
        };

        var issues = new List<InventoryIssue> { issue };
        var issuesMock = issues.BuildMockDbSet();

        _contextMock.Setup(c => c.InventoryIssues).Returns(issuesMock.Object);

        var command = new CompleteInventoryIssueCommand(issueId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already completed"));
    }
}
