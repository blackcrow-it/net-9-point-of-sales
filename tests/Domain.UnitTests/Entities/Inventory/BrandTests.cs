using Domain.Entities.Inventory;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Inventory;

public class BrandTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var brand = new Brand
        {
            Name = ""
        };

        // Act
        var act = () => brand.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WhenWebsiteIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var brand = new Brand
        {
            Name = "Samsung",
            Website = "not-a-valid-url"
        };

        // Act
        var act = () => brand.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Website must be a valid URL*");
    }

    [Fact]
    public void Validate_WhenWebsiteIsValid_ShouldNotThrow()
    {
        // Arrange
        var brand = new Brand
        {
            Name = "Samsung",
            Website = "https://www.samsung.com"
        };

        // Act
        var act = () => brand.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WhenWebsiteIsNull_ShouldNotThrow()
    {
        // Arrange
        var brand = new Brand
        {
            Name = "Samsung",
            Website = null
        };

        // Act
        var act = () => brand.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
