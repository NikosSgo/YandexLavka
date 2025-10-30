using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetOrderQuery(Guid OrderId) : IRequest<OrderDto>;

public class GetOrderQueryHandler : IRequestHandler<GetOrderQuery, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<GetOrderQueryHandler> _logger;

    public GetOrderQueryHandler(IOrderRepository orderRepository, ILogger<GetOrderQueryHandler> logger)
    {
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task<OrderDto> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting order {OrderId}", request.OrderId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId);

        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", request.OrderId);
            throw new Exception($"Order {request.OrderId} not found");
        }

        return MapToOrderDto(order);
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