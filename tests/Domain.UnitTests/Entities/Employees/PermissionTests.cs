using Domain.Entities.Employees;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Employees;

public class PermissionTests
{
    [Fact]
    public void Validate_WhenResourceIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var permission = new Permission
        {
            Resource = "",
            Action = "View"
        };

        // Act
        var act = () => permission.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Resource is required*");
    }

    [Fact]
    public void Validate_WhenActionIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var permission = new Permission
        {
            Resource = "Orders",
            Action = ""
        };

        // Act
        var act = () => permission.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Action is required*");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var permission = new Permission
        {
            Resource = "Orders",
            Action = "View"
        };

        // Act
        var act = () => permission.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
