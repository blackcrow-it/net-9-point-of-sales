using Domain.Entities.Employees;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Employees;

public class RoleTests
{
    [Fact]
    public void Validate_WhenNameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var role = new Role
        {
            Name = ""
        };

        // Act
        var act = () => role.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Name is required*");
    }

    [Fact]
    public void Validate_WithValidName_ShouldNotThrow()
    {
        // Arrange
        var role = new Role
        {
            Name = "Administrator",
            IsActive = true
        };

        // Act
        var act = () => role.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
