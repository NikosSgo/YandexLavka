using Microsoft.Extensions.Logging;
using OrderService.Application.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Services;

public class OrderStateMachine : IOrderStateMachine
{
    private static readonly IReadOnlyDictionary<OrderStatus, OrderStatus[]> AllowedTransitions =
        new Dictionary<OrderStatus, OrderStatus[]>
        {
            [OrderStatus.Initialized] = new[] { OrderStatus.AwaitingPayment, OrderStatus.Cancelled },
            [OrderStatus.AwaitingPayment] = new[] { OrderStatus.PaymentProcessing, OrderStatus.Cancelled },
            [OrderStatus.PaymentProcessing] = new[] { OrderStatus.PaymentFailed, OrderStatus.PaymentConfirmed },
            [OrderStatus.PaymentFailed] = new[] { OrderStatus.AwaitingPayment, OrderStatus.Cancelled },
            [OrderStatus.PaymentConfirmed] = new[] { OrderStatus.Picking, OrderStatus.Cancelled },
            [OrderStatus.Picking] = new[] { OrderStatus.Packing, OrderStatus.Cancelled },
            [OrderStatus.Packing] = new[] { OrderStatus.ReadyForDelivery, OrderStatus.Cancelled },
            [OrderStatus.ReadyForDelivery] = new[] { OrderStatus.OutForDelivery, OrderStatus.Cancelled },
            [OrderStatus.OutForDelivery] = new[] { OrderStatus.Delivered, OrderStatus.Cancelled },
            [OrderStatus.Delivered] = Array.Empty<OrderStatus>(),
            [OrderStatus.Cancelled] = Array.Empty<OrderStatus>(),
        };

    private readonly ILogger<OrderStateMachine> _logger;
    private readonly IReadOnlyDictionary<OrderStatus, IReadOnlyCollection<IOrderStateAction>> _actions;

    public OrderStateMachine(
        IEnumerable<IOrderStateAction> actions,
        ILogger<OrderStateMachine> logger)
    {
        _logger = logger;
        var materializedActions = (actions ?? Array.Empty<IOrderStateAction>()).ToArray();
        _actions = materializedActions
            .GroupBy(a => a.Status)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<IOrderStateAction>)g.ToList());
    }

    public async Task TransitionAsync(
        Order order,
        OrderStatus targetStatus,
        string? actor,
        string? notes,
        CancellationToken cancellationToken)
    {
        var allowed = GetNextStatuses(order.Status);
        if (!allowed.Contains(targetStatus))
        {
            throw new InvalidOperationException(
                $"Transition from {order.Status} to {targetStatus} is not allowed");
        }

        var previousStatus = order.Status;
        order.RegisterStageChange(targetStatus, actor, notes);
        _logger.LogInformation(
            "Order {OrderId} transitioned from {From} to {To}",
            order.Id,
            previousStatus,
            targetStatus);

        if (_actions.TryGetValue(targetStatus, out var actions))
        {
            foreach (var action in actions)
            {
                await action.OnEnterAsync(order, cancellationToken);
            }
        }
    }

    public IReadOnlyCollection<OrderStatus> GetNextStatuses(OrderStatus currentStatus) =>
        AllowedTransitions.TryGetValue(currentStatus, out var transitions)
            ? transitions
            : Array.Empty<OrderStatus>();
}

