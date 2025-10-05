using Application.Common.Interfaces;
using Application.Features.Reports.Queries;
using Domain.Entities.Sales;
using Domain.Entities.Store;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Reports.Queries;

public class SalesReportQueryHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly SalesReportQueryHandler _handler;

    public SalesReportQueryHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new SalesReportQueryHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidQuery_ShouldReturnSalesReport()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.RetailStore };

        var orders = new List<Order>
        {
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD001",
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                TotalAmount = 500000,
                TaxAmount = 50000,
                DiscountAmount = 10000,
                Status = OrderStatus.Completed
            },
            new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = "ORD002",
                StoreId = storeId,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                TotalAmount = 300000,
                TaxAmount = 30000,
                DiscountAmount = 5000,
                Status = OrderStatus.Completed
            }
        };

        var query = new SalesReportQuery(
            storeId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            "Day"
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Orders).Returns(orders.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.TotalSales.Should().Be(800000);
        result.Data.TotalOrders.Should().Be(2);
        result.Data.TotalTax.Should().Be(80000);
        result.Data.TotalDiscount.Should().Be(15000);
        result.Data.TimeSeries.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_InvalidStore_ShouldReturnFailure()
    {
        // Arrange
        var query = new SalesReportQuery(
            Guid.NewGuid(),
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            "Day"
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store>().BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Store not found");
    }

    [Fact]
    public async Task Handle_NoOrders_ShouldReturnZeroValues()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var store = new Store { Id = storeId, Code = "ST001", Name = "Main Store", Type = StoreType.RetailStore };

        var query = new SalesReportQuery(
            storeId,
            DateTime.UtcNow.AddDays(-7),
            DateTime.UtcNow,
            "Day"
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { store }.BuildMockDbSet().Object);
        _contextMock.Setup(c => c.Orders).Returns(new List<Order>().BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.TotalSales.Should().Be(0);
        result.Data.TotalOrders.Should().Be(0);
        result.Data.AverageOrderValue.Should().Be(0);
    }
}
