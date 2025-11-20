using OrderService.Domain.Enums;

namespace OrderService.Application.Dtos;

public record OrderStageHistoryDto(OrderStatus Status, DateTimeOffset ChangedAt, string? Actor, string? Notes);

