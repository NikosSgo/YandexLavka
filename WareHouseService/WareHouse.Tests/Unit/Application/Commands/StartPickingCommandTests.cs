using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using WareHouse.Application.Commands;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;
using Xunit;

namespace WareHouse.Tests.Unit.Application.Commands;

public class StartPickingCommandTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IPickingTaskRepository> _pickingTaskRepositoryMock;
    private readonly Mock<IStorageUnitRepository> _storageUnitRepositoryMock;
    private readonly Mock<ILogger<StartPickingCommandHandler>> _loggerMock;
    private readonly StartPickingCommandHandler _handler;

    public StartPickingCommandTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _pickingTaskRepositoryMock = new Mock<IPickingTaskRepository>();
        _storageUnitRepositoryMock = new Mock<IStorageUnitRepository>();
        _loggerMock = new Mock<ILogger<StartPickingCommandHandler>>();

        _handler = new StartPickingCommandHandler(
            _orderRepositoryMock.Object,
            _pickingTaskRepositoryMock.Object,
            _storageUnitRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenOrderExistsAndCanStartPicking_ShouldCreatePickingTask()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.NewGuid(), "picker-123", "Zone-A");
        var order = CreateTestOrder();
        var storageUnits = CreateTestStorageUnits();

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(command.OrderId))
            .ReturnsAsync(order);
        _pickingTaskRepositoryMock.Setup(x => x.GetActiveForOrderAsync(command.OrderId))
            .ReturnsAsync((PickingTask)null);
        _storageUnitRepositoryMock.Setup(x => x.GetUnitsForOrderAsync(command.OrderId))
            .ReturnsAsync(storageUnits);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().Be(command.OrderId);
        result.AssignedPicker.Should().Be(command.PickerId);

        _pickingTaskRepositoryMock.Verify(x => x.AddAsync(It.IsAny<PickingTask>()), Times.Once);
        _orderRepositoryMock.Verify(x => x.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.NewGuid(), "picker-123", "Zone-A");

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(command.OrderId))
            .ReturnsAsync((OrderAggregate)null);

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{command.OrderId}*");
    }

    [Fact]
    public async Task Handle_WhenInsufficientStock_ShouldThrowDomainException()
    {
        // Arrange
        var command = new StartPickingCommand(Guid.NewGuid(), "picker-123", "Zone-A");
        var order = CreateTestOrder();

        _orderRepositoryMock.Setup(x => x.GetByIdAsync(command.OrderId))
            .ReturnsAsync(order);
        _pickingTaskRepositoryMock.Setup(x => x.GetActiveForOrderAsync(command.OrderId))
            .ReturnsAsync((PickingTask)null);
        _storageUnitRepositoryMock.Setup(x => x.GetUnitsForOrderAsync(command.OrderId))
            .ReturnsAsync(new List<StorageUnit>()); // Нет доступных единиц

        // Act & Assert
        await _handler.Invoking(h => h.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<DomainException>()
            .WithMessage("*insufficient stock*");
    }

    private static OrderAggregate CreateTestOrder()
    {
        var orderId = Guid.NewGuid();
        var lines = new List<OrderLine>
        {
            new OrderLine(Guid.NewGuid(), "Product 1", "SKU-001", 2, 100m)
        };

        return new OrderAggregate(orderId, "customer-123", lines);
    }

    private static List<StorageUnit> CreateTestStorageUnits()
    {
        return new List<StorageUnit>
        {
            new StorageUnit(
                Guid.NewGuid(),
                "Product 1",
                "SKU-001",
                10,
                "A-01-01",
                "Zone-A"
            )
        };
    }
}