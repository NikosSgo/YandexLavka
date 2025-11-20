using OrderService.Domain.Enums;

namespace OrderService.API.Contracts;

public record AdvanceOrderStatusRequest(
    OrderStatus TargetStatus,
    string? Actor,
    string? Notes);

