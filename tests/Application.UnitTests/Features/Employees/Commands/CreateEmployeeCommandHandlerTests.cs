using Application.Common.Interfaces;
using Application.Features.Employees.Commands;
using Domain.Entities.Employees;
using Domain.Entities.Store;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Employees.Commands;

public class CreateEmployeeCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateEmployeeCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateEmployee()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        
        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.RetailStore };
        var role = new Role { Id = roleId, Name = "Cashier", IsActive = true };

        var command = new CreateEmployeeCommand(
            "john.doe",
            "SecurePass123!",
            "John Doe",
            "john@example.com",
            "0901234567",
            storeId,
            roleId,
            true
        );

        _contextMock.Setup(c => c.Users).Returns(new List<User>().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Roles).Returns(new List<Role> { role }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateUsername_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "john.doe",
            PasswordHash = "hash",
            FullName = "Existing User",
            Email = "existing@example.com"
        };

        var command = new CreateEmployeeCommand(
            "john.doe", // Duplicate
            "Password123!",
            "New User",
            "new@example.com",
            "0901234567",
            Guid.NewGuid(),
            Guid.NewGuid(),
            true
        );

        _contextMock.Setup(c => c.Users).Returns(new List<User> { existingUser }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Username"));
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "existing.user",
            PasswordHash = "hash",
            FullName = "Existing User",
            Email = "john@example.com"
        };

        var command = new CreateEmployeeCommand(
            "john.doe",
            "Password123!",
            "New User",
            "john@example.com", // Duplicate
            "0901234567",
            Guid.NewGuid(),
            Guid.NewGuid(),
            true
        );

        _contextMock.Setup(c => c.Users).Returns(new List<User> { existingUser }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Email"));
    }
}
