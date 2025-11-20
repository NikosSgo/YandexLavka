namespace OrderService.Application.Contracts;

public record OrderItemDto(
    string Sku,
    string Name,
    decimal Price,
    int Quantity);

