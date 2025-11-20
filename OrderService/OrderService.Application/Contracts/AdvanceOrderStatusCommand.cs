using OrderService.Domain.Enums;

namespace OrderService.Application.Contracts;

public record AdvanceOrderStatusCommand(
    Guid OrderId,
    OrderStatus TargetStatus,
    string? Actor,
    string? Notes);

