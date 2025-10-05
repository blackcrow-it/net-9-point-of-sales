using Application.Common.Interfaces;
using Application.Features.Customers.Commands;
using Domain.Entities.Customers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Customers.Commands;

public class AddLoyaltyPointsCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly AddLoyaltyPointsCommandHandler _handler;

    public AddLoyaltyPointsCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new AddLoyaltyPointsCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldAddLoyaltyPoints()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            CustomerNumber = "CUS001",
            Name = "John Doe",
            Phone = "0901234567",
            LoyaltyPoints = 100
        };

        var command = new AddLoyaltyPointsCommand(
            customerId,
            50,
            "Earned",
            "Purchase reward",
            null
        );

        _contextMock.Setup(c => c.Customers).Returns(new List<Customer> { customer }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidCustomer_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddLoyaltyPointsCommand(
            Guid.NewGuid(),
            50,
            "Earned",
            "Purchase reward",
            null
        );

        _contextMock.Setup(c => c.Customers).Returns(new List<Customer>().BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Customer not found");
    }

    [Fact]
    public async Task Handle_InvalidTransactionType_ShouldReturnFailure()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            CustomerNumber = "CUS001",
            Name = "John Doe",
            Phone = "0901234567",
            LoyaltyPoints = 100
        };

        var command = new AddLoyaltyPointsCommand(
            customerId,
            50,
            "InvalidType", // Invalid
            "Test",
            null
        );

        _contextMock.Setup(c => c.Customers).Returns(new List<Customer> { customer }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Invalid transaction type"));
    }

    [Fact]
    public async Task Handle_ZeroPoints_ShouldReturnFailure()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var customer = new Customer
        {
            Id = customerId,
            CustomerNumber = "CUS001",
            Name = "John Doe",
            Phone = "0901234567",
            LoyaltyPoints = 100
        };

        var command = new AddLoyaltyPointsCommand(
            customerId,
            0, // Zero points
            "Earned",
            "Test",
            null
        );

        _contextMock.Setup(c => c.Customers).Returns(new List<Customer> { customer }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("Points must be greater than 0"));
    }
}
