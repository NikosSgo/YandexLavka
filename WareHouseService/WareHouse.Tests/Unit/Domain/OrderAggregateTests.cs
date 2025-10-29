//using FluentAssertions;
//using WareHouse.Domain.Entities;
//using WareHouse.Domain.Enums;
//using WareHouse.Domain.Events;
//using WareHouse.Domain.Exceptions;
//using Xunit;

//namespace WareHouse.Tests.Unit.Domain;

//public class OrderAggregateTests
//{
//    [Fact]
//    public void CreateOrder_WithValidData_ShouldCreateOrderWithReceivedStatus()
//    {
//        // Arrange
//        var orderId = Guid.NewGuid();
//        var customerId = "customer-123";
//        var lines = new List<OrderLine>
//        {
//            new OrderLine(Guid.NewGuid(), "Product 1", "SKU-001", 2, 100m),
//            new OrderLine(Guid.NewGuid(), "Product 2", "SKU-002", 1, 200m)
//        };

//        // Act
//        var order = new OrderAggregate(orderId, customerId, lines);

//        // Assert
//        order.OrderId.Should().Be(orderId);
//        order.CustomerId.Should().Be(customerId);
//        order.Status.Should().Be(OrderStatus.Received);
//        order.Lines.Should().HaveCount(2);
//        order.DomainEvents.Should().ContainSingle(e => e is OrderReceivedEvent);
//    }

//    [Fact]
//    public void StartPicking_WhenOrderIsReceived_ShouldChangeStatusToPicking()
//    {
//        // Arrange
//        var order = CreateTestOrder();

//        // Act
//        order.StartPicking();

//        // Assert
//        order.Status.Should().Be(OrderStatus.Picking);
//        order.PickingStartedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
//        order.DomainEvents.Should().ContainSingle(e => e is OrderPickingStartedEvent);
//    }

//    [Fact]
//    public void StartPicking_WhenOrderIsNotReceived_ShouldThrowException()
//    {
//        // Arrange
//        var order = CreateTestOrder();
//        order.StartPicking(); // Переводим в Picking

//        // Act & Assert
//        order.Invoking(o => o.StartPicking())
//            .Should().Throw<DomainException>()
//            .WithMessage("*cannot start picking*");
//    }

//    [Fact]
//    public void CompletePicking_WithValidQuantities_ShouldChangeStatusToPicked()
//    {
//        // Arrange
//        var order = CreateTestOrder();
//        var productId = order.Lines.First().ProductId;
//        order.StartPicking();

//        var pickedQuantities = new Dictionary<Guid, int>
//        {
//            { productId, 2 }
//        };

//        // Act
//        order.CompletePicking(pickedQuantities);

//        // Assert
//        order.Status.Should().Be(OrderStatus.Picked);
//        order.PickingCompletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
//        order.Lines.First().QuantityPicked.Should().Be(2);
//        order.DomainEvents.Should().ContainSingle(e => e is OrderPickedEvent);
//    }

//    [Fact]
//    public void CompletePicking_WithInsufficientQuantities_ShouldThrowException()
//    {
//        // Arrange
//        var order = CreateTestOrder();
//        var productId = order.Lines.First().ProductId;
//        order.StartPicking();

//        var pickedQuantities = new Dictionary<Guid, int>
//        {
//            { productId, 1 } // Заказано 2, собрано 1
//        };

//        // Act & Assert
//        order.Invoking(o => o.CompletePicking(pickedQuantities))
//            .Should().Throw<DomainException>()
//            .WithMessage("*insufficient quantity*");
//    }

//    [Fact]
//    public void Cancel_WhenOrderIsReceived_ShouldChangeStatusToCancelled()
//    {
//        // Arrange
//        var order = CreateTestOrder();
//        var reason = "Customer cancelled";

//        // Act
//        order.Cancel(reason);

//        // Assert
//        order.Status.Should().Be(OrderStatus.Cancelled);
//        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledEvent);
//    }

//    [Fact]
//    public void AllItemsPicked_WhenAllItemsArePicked_ShouldReturnTrue()
//    {
//        // Arrange
//        var order = CreateTestOrder();
//        var productId = order.Lines.First().ProductId;

//        // Act
//        order.Lines.First().UpdatePickedQuantity(2);

//        // Assert
//        order.AllItemsPicked.Should().Be(true);
//    }

//    private static OrderAggregate CreateTestOrder()
//    {
//        var orderId = Guid.NewGuid();
//        var customerId = "customer-123";
//        var lines = new List<OrderLine>
//        {
//            new OrderLine(Guid.NewGuid(), "Product 1", "SKU-001", 2, 100m)
//        };

//        return new OrderAggregate(orderId, customerId, lines);
//    }
//}