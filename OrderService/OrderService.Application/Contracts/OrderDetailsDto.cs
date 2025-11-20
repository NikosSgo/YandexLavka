using OrderService.Domain.Enums;

namespace OrderService.Application.Contracts;

public record OrderDetailsDto(
    Guid Id,
    string OrderNumber,
    Guid UserId,
    AddressDto DeliveryAddress,
    IReadOnlyCollection<OrderItemDto> Items,
    Dictionary<string, string> Metadata,
    OrderStatus Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<OrderStageDto> StageHistory);

