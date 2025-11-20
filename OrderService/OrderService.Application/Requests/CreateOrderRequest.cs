using OrderService.Application.Dtos;

namespace OrderService.Application.Requests;

public record CreateOrderItemRequest(string Sku, string Name, decimal Price, int Quantity);

public record CreateOrderRequest(
    Guid UserId,
    DeliveryAddressDto DeliveryAddress,
    IReadOnlyCollection<CreateOrderItemRequest> Items,
    Dictionary<string, string>? Metadata);

