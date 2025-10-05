using Application.Common.Interfaces;
using Application.Features.Auth.Commands;
using Domain.Entities.Employees;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Auth.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _handler = new LoginCommandHandler(_contextMock.Object, _passwordHasherMock.Object, _jwtTokenGeneratorMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 12);
        var role = new Role { Id = Guid.NewGuid(), Name = "Cashier", IsActive = true };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = hashedPassword,
            IsActive = true,
            StoreId = Guid.NewGuid(),
            RoleId = role.Id,
            Role = role
        };

        var command = new LoginCommand("testuser", "password123");

        var users = new List<User> { user };
        var mockSet = users.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
        _passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtTokenGeneratorMock.Setup(j => j.GenerateAccessToken(It.IsAny<User>(), It.IsAny<List<string>>())).Returns("access-token");
        _jwtTokenGeneratorMock.Setup(j => j.GenerateRefreshToken()).Returns("refresh-token");
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.User.Id.Should().Be(user.Id);
        result.Data.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task Handle_InvalidUsername_ShouldReturnFailure()
    {
        // Arrange
        var command = new LoginCommand("nonexistent", "password123");

        var users = new List<User>();
        var mockSet = users.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Users).Returns(mockSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Handle_InvalidPassword_ShouldReturnFailure()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("correctpassword", workFactor: 12);
        var role = new Role { Id = Guid.NewGuid(), Name = "Cashier", IsActive = true };
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = hashedPassword,
            IsActive = true,
            RoleId = role.Id,
            Role = role
        };

        var command = new LoginCommand("testuser", "wrongpassword");

        var users = new List<User> { user };
        var mockSet = users.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Users).Returns(mockSet.Object);
        _passwordHasherMock.Setup(p => p.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Invalid username or password");
    }

    [Fact]
    public async Task Handle_InactiveUser_ShouldReturnFailure()
    {
        // Arrange
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword("password123", workFactor: 12);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            PasswordHash = hashedPassword,
            IsActive = false // Inactive user
        };

        var command = new LoginCommand("testuser", "password123");

        var users = new List<User> { user };
        var mockSet = users.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Users).Returns(mockSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("User account is inactive");
    }
}
