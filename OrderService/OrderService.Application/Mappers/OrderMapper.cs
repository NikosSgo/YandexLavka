using OrderService.Application.Contracts;
using OrderService.Domain.Entities;
using OrderService.Domain.ValueObjects;

namespace OrderService.Application.Mappers;

public static class OrderMapper
{
    public static OrderDetailsDto ToDetailsDto(Order order)
    {
        var address = new AddressDto(
            order.DeliveryAddress.Country,
            order.DeliveryAddress.City,
            order.DeliveryAddress.Street,
            order.DeliveryAddress.Building,
            order.DeliveryAddress.Apartment,
            order.DeliveryAddress.Comment);

        var items = order.Items
            .Select(i => new OrderItemDto(i.Sku, i.Name, i.Price, i.Quantity))
            .ToArray();

        var history = order.StageHistory
            .Select(s => new OrderStageDto(s.Status, s.ChangedAt, s.Actor, s.Notes))
            .ToArray();

        return new OrderDetailsDto(
            order.Id,
            order.OrderNumber,
            order.UserId,
            address,
            items,
            new Dictionary<string, string>(order.Metadata),
            order.Status,
            order.TotalAmount,
            order.CreatedAt,
            order.UpdatedAt,
            history);
    }

    public static Address ToDomain(AddressDto dto) =>
        new(
            dto.Country,
            dto.City,
            dto.Street,
            dto.Building,
            dto.Apartment,
            dto.Comment);

    public static IEnumerable<OrderItem> ToDomain(IEnumerable<OrderItemDto> items) =>
        items.Select(i => new OrderItem(i.Sku, i.Name, i.Price, i.Quantity));
}

