using OrderService.Domain.Enums;

namespace OrderService.Application.Requests;

public record AdvanceOrderStageRequest(
    OrderStatus TargetStage,
    string? Actor,
    string? Notes,
    Dictionary<string, string>? Payload);

