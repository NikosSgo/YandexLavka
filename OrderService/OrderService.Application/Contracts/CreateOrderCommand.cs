namespace OrderService.Application.Contracts;

public record CreateOrderCommand(
    Guid UserId,
    AddressDto DeliveryAddress,
    IReadOnlyCollection<OrderItemDto> Items,
    Dictionary<string, string>? Metadata);

