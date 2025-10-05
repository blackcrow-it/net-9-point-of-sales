using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class StocktakeTests
{
    [Fact]
    public void Start_WhenScheduled_ShouldSetStatusToInProgress()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.Scheduled
        };

        // Act
        stocktake.Start();

        // Assert
        stocktake.Status.Should().Be(StocktakeStatus.InProgress);
    }

    [Fact]
    public void Start_WhenNotScheduled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.InProgress
        };

        // Act
        var act = () => stocktake.Start();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WhenInProgress_ShouldSetStatusToCompletedAndSetDate()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.InProgress
        };

        // Act
        stocktake.Complete();

        // Assert
        stocktake.Status.Should().Be(StocktakeStatus.Completed);
        stocktake.CompletedDate.Should().NotBeNull();
        stocktake.CompletedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Complete_WhenNotInProgress_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.Scheduled
        };

        // Act
        var act = () => stocktake.Complete();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_WhenNotCompleted_ShouldSetStatusToCancelled()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.Scheduled
        };

        // Act
        stocktake.Cancel();

        // Assert
        stocktake.Status.Should().Be(StocktakeStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.Cancelled
        };

        // Act
        var act = () => stocktake.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Stocktake is already cancelled");
    }

    [Fact]
    public void Cancel_WhenCompleted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            Status = StocktakeStatus.Completed
        };

        // Act
        var act = () => stocktake.Cancel();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel completed stocktake");
    }

    [Fact]
    public void Validate_WhenStocktakeNumberIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            StocktakeNumber = "",
            ScheduledDate = DateTime.UtcNow
        };

        // Act
        var act = () => stocktake.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Stocktake number is required*");
    }

    [Fact]
    public void Validate_WhenCompletedWithoutDate_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            StocktakeNumber = "STK-001",
            Status = StocktakeStatus.Completed,
            ScheduledDate = DateTime.UtcNow,
            CompletedDate = null
        };

        // Act
        var act = () => stocktake.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Completed date is required when status is Completed");
    }

    [Fact]
    public void Validate_WhenCompletedDateBeforeScheduled_ShouldThrowArgumentException()
    {
        // Arrange
        var stocktake = new Stocktake
        {
            StocktakeNumber = "STK-001",
            Status = StocktakeStatus.Completed,
            ScheduledDate = DateTime.UtcNow,
            CompletedDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var act = () => stocktake.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Completed date must be >= scheduled date*");
    }

    [Theory]
    [InlineData(StocktakeStatus.Scheduled)]
    [InlineData(StocktakeStatus.InProgress)]
    [InlineData(StocktakeStatus.Completed)]
    [InlineData(StocktakeStatus.Cancelled)]
    public void StocktakeStatus_ShouldHaveAllExpectedValues(StocktakeStatus status)
    {
        // Assert
        status.Should().BeDefined();
    }
}
