using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Workflow;

public class OrderStateMachine
{
    private readonly Dictionary<OrderStatus, OrderStageConfiguration> _configurations;

    public OrderStateMachine(IEnumerable<OrderStageConfiguration> configurations)
    {
        _configurations = configurations?.ToDictionary(c => c.Stage)
            ?? throw new ArgumentNullException(nameof(configurations));
    }

    public bool CanTransition(OrderStatus from, OrderStatus to)
    {
        return _configurations.TryGetValue(from, out var definition)
            && definition.AllowedTransitions.Contains(to);
    }

    public async Task ApplyAsync(
        Order order,
        OrderStatus targetStage,
        StageActionContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_configurations.TryGetValue(order.Status, out var currentStage))
        {
            throw new InvalidOperationException($"Stage '{order.Status}' is not configured.");
        }

        if (!currentStage.AllowedTransitions.Contains(targetStage))
        {
            throw new InvalidOperationException($"Transition {order.Status} -> {targetStage} is not allowed.");
        }

        if (!_configurations.TryGetValue(targetStage, out var targetDefinition))
        {
            throw new InvalidOperationException($"Target stage '{targetStage}' is not configured.");
        }

        await targetDefinition.Action.ExecuteAsync(order, context, cancellationToken);
        order.RegisterStageChange(targetStage, context.Actor, context.Notes);
    }
}

