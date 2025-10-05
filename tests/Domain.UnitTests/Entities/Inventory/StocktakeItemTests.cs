using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class StocktakeItemTests
{
    [Fact]
    public void CalculateVariance_ShouldCalculateCorrectly()
    {
        // Arrange
        var item = new StocktakeItem
        {
            SystemQuantity = 100m,
            CountedQuantity = 95m
        };

        // Act
        item.CalculateVariance();

        // Assert
        item.Variance.Should().Be(-5m); // Short by 5
    }

    [Fact]
    public void CalculateVariance_WhenCountedMoreThanSystem_ShouldBePositive()
    {
        // Arrange
        var item = new StocktakeItem
        {
            SystemQuantity = 100m,
            CountedQuantity = 110m
        };

        // Act
        item.CalculateVariance();

        // Assert
        item.Variance.Should().Be(10m); // Over by 10
    }

    [Fact]
    public void CalculateVariance_WhenCountedEqualSystem_ShouldBeZero()
    {
        // Arrange
        var item = new StocktakeItem
        {
            SystemQuantity = 100m,
            CountedQuantity = 100m
        };

        // Act
        item.CalculateVariance();

        // Assert
        item.Variance.Should().Be(0m);
    }
}
