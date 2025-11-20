using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using OrderService.Domain.Enums;

namespace OrderService.Infrastructure.Persistence.Documents;

public class OrderDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public string OrderNumber { get; set; } = default!;

    public Guid UserId { get; set; }

    public AddressDocument DeliveryAddress { get; set; } = default!;

    public List<OrderItemDocument> Items { get; set; } = new();

    public Dictionary<string, string> Metadata { get; set; } = new();

    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; }

    public List<OrderStageHistoryDocument> StageHistory { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}

public class AddressDocument
{
    public string Country { get; set; } = default!;
    public string City { get; set; } = default!;
    public string Street { get; set; } = default!;
    public string Building { get; set; } = default!;
    public string? Apartment { get; set; }
    public string? Comment { get; set; }
}

public class OrderItemDocument
{
    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderStageHistoryDocument
{
    [BsonRepresentation(BsonType.String)]
    public OrderStatus Status { get; set; }

    public DateTimeOffset ChangedAt { get; set; }

    public string? Actor { get; set; }

    public string? Notes { get; set; }
}

