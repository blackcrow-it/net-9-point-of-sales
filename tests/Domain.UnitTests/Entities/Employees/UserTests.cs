using Domain.Entities.Employees;
using FluentAssertions;

namespace Domain.UnitTests.Entities.Employees;

public class UserTests
{
    [Fact]
    public void UpdateRefreshToken_WithValidData_ShouldSetTokenAndExpiration()
    {
        // Arrange
        var user = new User();
        var token = "refresh-token-123";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        user.UpdateRefreshToken(token, expiresAt);

        // Assert
        user.RefreshToken.Should().Be(token);
        user.RefreshTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void UpdateRefreshToken_WithEmptyToken_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User();

        // Act
        var act = () => user.UpdateRefreshToken("", DateTime.UtcNow.AddDays(1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Token is required*");
    }

    [Fact]
    public void UpdateRefreshToken_WithPastExpiration_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User();

        // Act
        var act = () => user.UpdateRefreshToken("token", DateTime.UtcNow.AddDays(-1));

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Expiration must be in the future*");
    }

    [Fact]
    public void MarkLastLogin_ShouldSetLastLoginTime()
    {
        // Arrange
        var user = new User();

        // Act
        user.MarkLastLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Validate_WhenUsernameIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Username = "",
            PasswordHash = "hash123"
        };

        // Act
        var act = () => user.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Username is required*");
    }

    [Fact]
    public void Validate_WhenPasswordHashIsEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Username = "user123",
            PasswordHash = ""
        };

        // Act
        var act = () => user.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Password hash is required*");
    }

    [Fact]
    public void Validate_WhenPhoneIsInvalid_ShouldThrowArgumentException()
    {
        // Arrange
        var user = new User
        {
            Username = "user123",
            PasswordHash = "hash123",
            Phone = "12345" // Invalid: not 10 digits
        };

        // Act
        var act = () => user.Validate();

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Phone must be 10 digits (Vietnamese format)*");
    }

    [Fact]
    public void Validate_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var user = new User
        {
            Username = "user123",
            PasswordHash = "hash123",
            Phone = "0987654321"
        };

        // Act
        var act = () => user.Validate();

        // Assert
        act.Should().NotThrow();
    }
}
