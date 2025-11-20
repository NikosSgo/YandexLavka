namespace OrderService.API.Contracts;

public record CreateOrderRequest(
    Guid UserId,
    AddressRequest DeliveryAddress,
    IReadOnlyCollection<OrderItemRequest> Items,
    Dictionary<string, string>? Metadata);

