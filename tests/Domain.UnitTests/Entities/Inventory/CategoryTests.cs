using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class CategoryTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var category = new Category
        {
            Name = "",
            DisplayOrder = 1
        };

        // Act
        var act = () => category.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WhenDisplayOrderIsNegative_ShouldThrowArgumentException()
    {
        // Arrange
        var category = new Category
        {
            Name = "Electronics",
            DisplayOrder = -1
        };

        // Act
        var act = () => category.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Display order must be >= 0*");
    }

    [Fact]
    public void Validate_WhenCategoryIsItsOwnParent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category
        {
            Id = categoryId,
            ParentId = categoryId,
            Name = "Electronics",
            DisplayOrder = 1
        };

        // Act
        var act = () => category.Validate();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Category cannot be its own parent");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var category = new Category
        {
            Name = "Electronics",
            DisplayOrder = 1,
            IsActive = true
        };

        // Act
        var act = () => category.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
