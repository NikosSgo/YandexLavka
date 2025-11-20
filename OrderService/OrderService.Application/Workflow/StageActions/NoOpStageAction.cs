using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Workflow.StageActions;

public class NoOpStageAction : IOrderStageAction
{
    public NoOpStageAction(OrderStatus stage)
    {
        Stage = stage;
    }

    public OrderStatus Stage { get; }

    public Task ExecuteAsync(Order order, StageActionContext context, CancellationToken cancellationToken)
    {
        // Intentionally empty, serves as an extension point for future logic.
        return Task.CompletedTask;
    }
}

