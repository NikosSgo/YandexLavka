using OrderService.Domain.Enums;

namespace OrderService.Application.Workflow;

public class OrderStageConfiguration
{
    public OrderStatus Stage { get; }
    public IReadOnlyCollection<OrderStatus> AllowedTransitions { get; }
    public IOrderStageAction Action { get; }

    public OrderStageConfiguration(OrderStatus stage, IEnumerable<OrderStatus> allowedTransitions, IOrderStageAction action)
    {
        Stage = stage;
        AllowedTransitions = allowedTransitions?.Distinct().ToArray()
            ?? throw new ArgumentNullException(nameof(allowedTransitions));
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}

