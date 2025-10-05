using Application.Common.Interfaces;
using Application.Features.Customers.Commands;
using Domain.Entities.Customers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Customers.Commands;

public class CreateCustomerCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateCustomerCommandHandler _handler;

    public CreateCustomerCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateCustomerCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateCustomer()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var command = new CreateCustomerCommand(
            Code: "CUS001",
            Name: "Nguyễn Văn A",
            Phone: "0901234567",
            Email: "test@example.com",
            CustomerGroupId: null,
            Address: "123 Test Street",
            StoreId: storeId,
            IsActive: true
        );

        var customers = new List<Customer>();
        var mockSet = customers.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Customers).Returns(mockSet.Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicatePhone_ShouldReturnFailure()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerNumber = "CUS001",
            Name = "Existing Customer",
            Phone = "0901234567",
            Email = "existing@example.com"
        };

        var command = new CreateCustomerCommand(
            Code: "CUS002",
            Name: "New Customer",
            Phone: "0901234567", // Duplicate phone
            Email: "new@example.com",
            CustomerGroupId: null,
            Address: "456 Test Street",
            StoreId: Guid.NewGuid(),
            IsActive: true
        );

        var customers = new List<Customer> { existingCustomer };
        var mockSet = customers.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Customers).Returns(mockSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_DuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var existingCustomer = new Customer
        {
            Id = Guid.NewGuid(),
            CustomerNumber = "CUS001",
            Name = "Existing Customer",
            Phone = "0902222222",
            Email = "test@example.com"
        };

        var command = new CreateCustomerCommand(
            Code: "CUS002",
            Name: "New Customer",
            Phone: "0903333333",
            Email: "test@example.com", // Duplicate email
            CustomerGroupId: null,
            Address: "456 Test Street",
            StoreId: Guid.NewGuid(),
            IsActive: true
        );

        var customers = new List<Customer> { existingCustomer };
        var mockSet = customers.BuildMockDbSet();
        
        _contextMock.Setup(c => c.Customers).Returns(mockSet.Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }
}
