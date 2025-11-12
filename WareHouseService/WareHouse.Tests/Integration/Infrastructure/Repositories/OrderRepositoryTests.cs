//using FluentAssertions;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using WareHouse.Domain.Entities;
//using WareHouse.Domain.Enums;
//using WareHouse.Domain.Interfaces;
//using WareHouse.Infrastructure.Data;
//using WareHouse.Infrastructure.Data.Repositories;
//using Xunit;

//namespace WareHouse.Tests.Integration.Infrastructure.Repositories;

//public class OrderRepositoryTests : IAsyncLifetime
//{
//    private readonly ApplicationDbContext _context;
//    private readonly IOrderRepository _orderRepository;
//    private Guid _testOrderId;

//    public OrderRepositoryTests()
//    {
//        var serviceProvider = new ServiceCollection()
//            .AddEntityFrameworkInMemoryDatabase()
//            .BuildServiceProvider();

//        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//            .UseInMemoryDatabase("TestDatabase")
//            .UseInternalServiceProvider(serviceProvider)
//            .Options;

//        _context = new ApplicationDbContext(options);
//        _orderRepository = new OrderRepository(_context);
//    }

//    [Fact]
//    public async Task GetByIdAsync_WhenOrderExists_ShouldReturnOrder()
//    {
//        // Act
//        var order = await _orderRepository.GetByIdAsync(_testOrderId);

//        // Assert
//        order.Should().NotBeNull();
//        order.OrderId.Should().Be(_testOrderId);
//        order.Lines.Should().HaveCount(1);
//    }

//    [Fact]
//    public async Task GetOrdersByStatusAsync_ShouldReturnFilteredOrders()
//    {
//        // Act
//        var orders = await _orderRepository.GetOrdersByStatusAsync(OrderStatus.Received);

//        // Assert
//        orders.Should().ContainSingle(o => o.OrderId == _testOrderId);
//    }

//    [Fact]
//    public async Task AddAsync_ShouldAddOrderToDatabase()
//    {
//        // Arrange
//        var newOrder = new OrderAggregate(
//            Guid.NewGuid(),
//            "test-customer",
//            new List<OrderLine>
//            {
//                new OrderLine(Guid.NewGuid(), "New Product", "SKU-NEW", 1, 50m)
//            }
//        );

//        // Act
//        await _orderRepository.AddAsync(newOrder);
//        await _orderRepository.SaveChangesAsync();

//        // Assert
//        var retrievedOrder = await _orderRepository.GetByIdAsync(newOrder.OrderId);
//        retrievedOrder.Should().NotBeNull();
//        retrievedOrder.CustomerId.Should().Be("test-customer");
//    }

//    public async Task InitializeAsync()
//    {
//        await _context.Database.EnsureCreatedAsync();

//        // Создаем тестовый заказ
//        var order = new OrderAggregate(
//            Guid.NewGuid(),
//            "test-customer",
//            new List<OrderLine>
//            {
//                new OrderLine(Guid.NewGuid(), "Test Product", "SKU-TEST", 1, 100m)
//            }
//        );

//        await _orderRepository.AddAsync(order);
//        await _orderRepository.SaveChangesAsync();

//        _testOrderId = order.OrderId;
//    }

//    public async Task DisposeAsync()
//    {
//        await _context.Database.EnsureDeletedAsync();
//        _context.Dispose();
//    }
//}