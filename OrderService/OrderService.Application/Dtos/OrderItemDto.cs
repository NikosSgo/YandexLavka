namespace OrderService.Application.Dtos;

public record OrderItemDto(string Sku, string Name, decimal Price, int Quantity);

