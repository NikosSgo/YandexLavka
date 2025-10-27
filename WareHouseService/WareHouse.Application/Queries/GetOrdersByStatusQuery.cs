using MediatR;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetOrdersByStatusQuery(string Status) : IRequest<List<OrderDto>>;

public class GetOrdersByStatusQueryHandler : IRequestHandler<GetOrdersByStatusQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersByStatusQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> Handle(GetOrdersByStatusQuery request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var status))
            throw new DomainException($"Invalid order status: {request.Status}");

        var orders = await _orderRepository.GetOrdersByStatusAsync(status);
        return orders.Select(OrderDto.FromEntity).ToList();
    }
}