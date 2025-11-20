using OrderService.Domain.Entities;
using OrderService.Domain.Enums;

namespace OrderService.Application.Workflow;

public interface IOrderStageAction
{
    OrderStatus Stage { get; }
    Task ExecuteAsync(Order order, StageActionContext context, CancellationToken cancellationToken);
}

public sealed record StageActionContext(
    OrderStatus TargetStage,
    string? Actor,
    string? Notes,
    Dictionary<string, string>? Payload);

