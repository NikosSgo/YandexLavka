using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Abstractions;

public interface IOrderStateAction
{
    OrderStatus Status { get; }

    Task OnEnterAsync(Order order, CancellationToken cancellationToken);
}

