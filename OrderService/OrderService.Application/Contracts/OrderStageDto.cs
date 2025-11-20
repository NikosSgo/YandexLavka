using OrderService.Domain.Enums;

namespace OrderService.Application.Contracts;

public record OrderStageDto(
    OrderStatus Status,
    DateTimeOffset ChangedAt,
    string? Actor,
    string? Notes);

