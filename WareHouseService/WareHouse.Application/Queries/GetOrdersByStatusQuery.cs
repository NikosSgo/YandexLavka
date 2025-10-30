using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetOrdersByStatusQuery(string Status) : IRequest<List<OrderDto>>;

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrdersByStatusQueryHandler> _logger;

    public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository, ILogger<GetOrdersByStatusQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting orders with status {Status}", request.Status);

        // Если статус не указан, возвращаем все заказы
        if (string.IsNullOrEmpty(request.Status) || request.Status.ToLower() == "all")
        {
            var allOrders = await _orderRepository.GetAllAsync();
            return allOrders.Select(MapToOrderDto).ToList();
        }

        // Конвертируем строку в enum
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
        {
            _logger.LogWarning("Invalid order status: {Status}", request.Status);
            return new List<OrderDto>();
        }

        var orders = await _orderRepository.GetByStatusAsync(status);

        return orders.Select(MapToOrderDto).ToList();
    }

    private OrderDto MapToOrderDto(OrderAggregate order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            PickingStartedAt = order.PickingStartedAt,
            PickingCompletedAt = order.PickingCompletedAt,
            Lines = order.Lines.Select(line => new OrderLineDto
            {
                ProductId = line.ProductId,
                ProductName = line.ProductName,
                Sku = line.Sku,
                QuantityOrdered = line.QuantityOrdered,
                QuantityPicked = line.QuantityPicked,
                UnitPrice = line.UnitPrice,
                TotalPrice = line.TotalPrice,
                IsFullyPicked = line.IsFullyPicked
            }).ToList(),
            TotalAmount = order.TotalAmount
        };
    }
}