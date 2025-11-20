using OrderService.Domain.Entities;
using OrderService.Domain.Enums;
using OrderService.Domain.ValueObjects;
using OrderService.Infrastructure.Persistence.Documents;

namespace OrderService.Infrastructure.Persistence.Mappers;

internal static class OrderDocumentMapper
{
    public static OrderDocument ToDocument(Order order)
    {
        return new OrderDocument
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            DeliveryAddress = new AddressDocument
            {
                Country = order.DeliveryAddress.Country,
                City = order.DeliveryAddress.City,
                Street = order.DeliveryAddress.Street,
                Building = order.DeliveryAddress.Building,
                Apartment = order.DeliveryAddress.Apartment,
                Comment = order.DeliveryAddress.Comment
            },
            Items = order.Items.Select(i => new OrderItemDocument
            {
                Sku = i.Sku,
                Name = i.Name,
                Price = i.Price,
                Quantity = i.Quantity
            }).ToList(),
            Metadata = new Dictionary<string, string>(order.Metadata),
            Status = order.Status,
            StageHistory = order.StageHistory.Select(s => new OrderStageHistoryDocument
            {
                Status = s.Status,
                ChangedAt = s.ChangedAt,
                Actor = s.Actor,
                Notes = s.Notes
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        };
    }

    public static Order? ToDomain(OrderDocument? document)
    {
        if (document is null)
        {
            return null;
        }

        if (document.DeliveryAddress is null)
        {
            throw new InvalidOperationException("Order document is missing delivery address");
        }

        var address = new Address(
            document.DeliveryAddress.Country,
            document.DeliveryAddress.City,
            document.DeliveryAddress.Street,
            document.DeliveryAddress.Building,
            document.DeliveryAddress.Apartment,
            document.DeliveryAddress.Comment);

        var items = document.Items
            .Select(i => new OrderItem(i.Sku, i.Name, i.Price, i.Quantity))
            .ToArray();

        var stageHistory = document.StageHistory
            .Select(s => new OrderStageHistory(s.Status, s.ChangedAt, s.Actor, s.Notes))
            .ToArray();

        return Order.Restore(
            document.Id,
            document.OrderNumber,
            document.UserId,
            address,
            items,
            document.Metadata,
            document.Status,
            stageHistory,
            document.CreatedAt,
            document.UpdatedAt);
    }
}

