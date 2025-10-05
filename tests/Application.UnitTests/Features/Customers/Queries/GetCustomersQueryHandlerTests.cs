using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Customers.Queries;
using Domain.Entities.Customers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Customers.Queries;

public class GetCustomersQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly GetCustomersQueryHandler _handler;

    public GetCustomersQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetCustomersQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_WithActiveFilter_ShouldReturnOnlyActiveCustomers()
    {
        // Arrange
        var customers = new List<Customer>
        {
            new() { Id = Guid.NewGuid(), CustomerNumber = "CUS001", Name = "Active Customer", IsActive = true },
            new() { Id = Guid.NewGuid(), CustomerNumber = "CUS002", Name = "Inactive Customer", IsActive = false }
        };

        var mockSet = customers.BuildMockDbSet();
        _contextMock.Setup(c => c.Customers).Returns(mockSet.Object);

        var query = new GetCustomersQuery(
            CustomerGroupId: null,
            IsActive: true,
            PageNumber: 1,
            PageSize: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var customers = new List<Customer>();
        for (int i = 1; i <= 15; i++)
        {
            customers.Add(new Customer
            {
                Id = Guid.NewGuid(),
                CustomerNumber = $"CUS{i:D3}",
                Name = $"Customer {i}",
                IsActive = true
            });
        }

        var mockSet = customers.BuildMockDbSet();
        _contextMock.Setup(c => c.Customers).Returns(mockSet.Object);

        var query = new GetCustomersQuery(
            CustomerGroupId: null,
            IsActive: null,
            PageNumber: 2,
            PageSize: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(15);
        result.Data.PageNumber.Should().Be(2);
        result.Data.Items.Count.Should().Be(5); // Remaining 5 items on page 2
    }
}
