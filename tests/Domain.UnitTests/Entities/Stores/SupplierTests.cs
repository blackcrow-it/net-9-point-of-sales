using Domain.Entities.Store;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Stores;

public class SupplierTests
{
    [Fact]
    public void Validate_WhenCodeIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Code = "",
            Name = "ABC Supplier"
        };

        // Act
        var act = () => supplier.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Code is required*");
    }

    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var supplier = new Supplier
        {
            Code = "SUP001",
            Name = ""
        };

        // Act
        var act = () => supplier.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var supplier = new Supplier
        {
            Code = "SUP001",
            Name = "ABC Supplier",
            ContactPerson = "John Doe",
            Phone = "0123456789",
            IsActive = true
        };

        // Act
        var act = () => supplier.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
