//using FluentAssertions;
//using Microsoft.AspNetCore.Mvc.Testing;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using System.Net;
//using System.Net.Http.Json;
//using WareHouse.API;
//using WareHouse.Application.DTOs;
//using WareHouse.Domain.Entities;
//using WareHouse.Domain.Enums;
//using WareHouse.Domain.Interfaces;
//using Xunit;

//namespace WareHouse.Tests.Integration.API;

//public class OrdersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IAsyncLifetime
//{
//    private readonly WebApplicationFactory<Program> _factory;
//    private readonly HttpClient _client;
//    private readonly IOrderRepository _orderRepository;
//    private Guid _testOrderId;

//    public OrdersControllerIntegrationTests(WebApplicationFactory<Program> factory)
//    {
//        _factory = factory.WithWebHostBuilder(builder =>
//        {
//            builder.ConfigureServices(services =>
//            {
//                // Можно добавать моки или тестовую БД
//            });
//        });

//        _client = _factory.CreateClient();

//        var scope = _factory.Services.CreateScope();
//        _orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
//    }

//    [Fact]
//    public async Task GetOrder_WhenOrderExists_ShouldReturnOrder()
//    {
//        // Arrange
//        var orderId = _testOrderId;

//        // Act
//        var response = await _client.GetAsync($"/api/orders/{orderId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);

//        var order = await response.Content.ReadFromJsonAsync<OrderDto>();
//        order.Should().NotBeNull();
//        order.OrderId.Should().Be(orderId);
//    }

//    [Fact]
//    public async Task GetOrder_WhenOrderNotExists_ShouldReturnNotFound()
//    {
//        // Arrange
//        var nonExistentOrderId = Guid.NewGuid();

//        // Act
//        var response = await _client.GetAsync($"/api/orders/{nonExistentOrderId}");

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
//    }

//    [Fact]
//    public async Task StartPicking_WhenValidRequest_ShouldReturnPickingTask()
//    {
//        // Arrange
//        var orderId = _testOrderId;
//        var request = new
//        {
//            PickerId = "integration-picker-1",
//            Zone = "Integration-Zone"
//        };

//        // Act
//        var response = await _client.PostAsJsonAsync($"/api/orders/{orderId}/start-picking", request);

//        // Assert
//        response.StatusCode.Should().Be(HttpStatusCode.OK);

//        var pickingTask = await response.Content.ReadFromJsonAsync<PickingTaskDto>();
//        pickingTask.Should().NotBeNull();
//        pickingTask.OrderId.Should().Be(orderId);
//        pickingTask.AssignedPicker.Should().Be(request.PickerId);
//    }

//    public async Task InitializeAsync()
//    {
//        // Создаем тестовый заказ
//        var order = new OrderAggregate(
//            Guid.NewGuid(),
//            "integration-customer-1",
//            new List<OrderLine>
//            {
//                new OrderLine(Guid.NewGuid(), "Integration Product", "SKU-INT-001", 1, 100m)
//            }
//        );

//        await _orderRepository.AddAsync(order);
//        await _orderRepository.SaveChangesAsync();

//        _testOrderId = order.OrderId;
//    }

//    public async Task DisposeAsync()
//    {
//        // Очищаем тестовые данные
//        var order = await _orderRepository.GetByIdAsync(_testOrderId);
//        if (order != null)
//        {
//            await _orderRepository.DeleteAsync(order);
//            await _orderRepository.SaveChangesAsync();
//        }

//        _client.Dispose();
//    }
//}