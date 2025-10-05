using Application.Common.Interfaces;
using Application.Features.Customers.Queries;
using Domain.Entities.Customers;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Customers.Queries;

public class SearchCustomersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly SearchCustomersQueryHandler _handler;

    public SearchCustomersQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new SearchCustomersQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_SearchByPhone_ShouldReturnMatchingCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                CustomerNumber = "CUS001",
                Name = "John Doe",
                Phone = "0901234567",
                Email = "john@example.com"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                CustomerNumber = "CUS002",
                Name = "Jane Smith",
                Phone = "0987654321",
                Email = "jane@example.com"
            }
        };

        var query = new SearchCustomersQuery("0901234567");

        _contextMock.Setup(c => c.Customers).Returns(customers.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.First().Phone.Should().Be("0901234567");
    }

    [Fact]
    public async Task Handle_SearchByName_ShouldReturnMatchingCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                CustomerNumber = "CUS001",
                Name = "John Doe",
                Phone = "0901234567",
                Email = "john@example.com"
            },
            new Customer
            {
                Id = Guid.NewGuid(),
                CustomerNumber = "CUS002",
                Name = "John Smith",
                Phone = "0987654321",
                Email = "jsmith@example.com"
            }
        };

        var query = new SearchCustomersQuery("John");

        _contextMock.Setup(c => c.Customers).Returns(customers.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.Should().AllSatisfy(c => c.Name.Should().Contain("John"));
    }

    [Fact]
    public async Task Handle_NoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.NewGuid(),
                CustomerNumber = "CUS001",
                Name = "John Doe",
                Phone = "0901234567",
                Email = "john@example.com"
            }
        };

        var query = new SearchCustomersQuery("nonexistent");

        _contextMock.Setup(c => c.Customers).Returns(customers.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEmpty();
    }
}
