using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Abstractions;

public interface IOrderStateMachine
{
    Task TransitionAsync(
        Order order,
        OrderStatus targetStatus,
        string? actor,
        string? notes,
        CancellationToken cancellationToken);

    IReadOnlyCollection<OrderStatus> GetNextStatuses(OrderStatus currentStatus);
}

