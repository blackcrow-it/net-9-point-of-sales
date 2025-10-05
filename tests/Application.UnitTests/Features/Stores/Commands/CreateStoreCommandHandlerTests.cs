using Application.Common.Interfaces;
using Application.Features.Stores.Commands;
using Domain.Entities.Store;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;

namespace Application.UnitTests.Features.Stores.Commands;

public class CreateStoreCommandHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly CreateStoreCommandHandler _handler;

    public CreateStoreCommandHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateStoreCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateStore()
    {
        // Arrange
        var command = new CreateStoreCommand(
            "ST001",
            "Main Store",
            StoreType.RetailStore,
            "123 Main St",
            "0901234567",
            "store@example.com",
            true
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store>().BuildMockDbSet().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateCode_ShouldReturnFailure()
    {
        // Arrange
        var existingStore = new Store
        {
            Id = Guid.NewGuid(),
            Code = "ST001",
            Name = "Existing Store",
            Type = StoreType.RetailStore
        };

        var command = new CreateStoreCommand(
            "ST001", // Duplicate code
            "New Store",
            StoreType.Warehouse,
            "456 New St",
            "0902222222",
            "newstore@example.com",
            true
        );

        _contextMock.Setup(c => c.Stores).Returns(new List<Store> { existingStore }.BuildMockDbSet().Object);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ST001"));
    }
}
