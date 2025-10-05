using Domain.Entities.Store;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Stores;

public class StoreTests
{
    [Fact]
    public void Validate_WhenCodeIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var store = new Store
        {
            Code = "",
            Name = "Main Store"
        };

        // Act
        var act = () => store.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code is required*");
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var store = new Store
        {
            Code = "STORE001",
            Name = ""
        };

        // Act
        var act = () => store.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var store = new Store
        {
            Code = "STORE001",
            Name = "Main Store",
            Type = StoreType.RetailStore,
            IsActive = true
        };

        // Act
        var act = () => store.Validate();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(StoreType.RetailStore)]
    [InlineData(StoreType.Warehouse)]
    [InlineData(StoreType.Both)]
    public void StoreType_ShouldHaveAllExpectedValues(StoreType type)
    {
        // Assert
        type.Should().BeDefined();
    }
}
