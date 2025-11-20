using OrderService.Domain.Enums;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Dtos;

public record OrderDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    OrderStatus Status,
    Address DeliveryAddress,
    decimal TotalAmount,
    IReadOnlyCollection<OrderItemDto> Items,
    IReadOnlyCollection<OrderStageHistoryDto> StageHistory,
    IReadOnlyDictionary<string, string> Metadata,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

