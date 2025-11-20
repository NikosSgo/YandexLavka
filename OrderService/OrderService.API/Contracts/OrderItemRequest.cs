namespace OrderService.API.Contracts;

public record OrderItemRequest(
    string Sku,
    string Name,
    decimal Price,
    int Quantity);

