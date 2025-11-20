using OrderService.Domain.Enums;
using OrderService.Domain.ValueObjects;

namespace OrderService.Domain.Entities;

public class Order
{
    private readonly List<OrderItem> _items = new();
    private readonly List<OrderStageHistory> _stageHistory = new();

    private Order() { }

    public Order(
        Guid id,
        Guid userId,
        Address deliveryAddress,
        IEnumerable<OrderItem> items,
        Dictionary<string, string>? metadata = null)
    {
        if (items is null || !items.Any())
        {
            throw new ArgumentException("Order must contain at least one item", nameof(items));
        }

        Id = id == Guid.Empty ? Guid.NewGuid() : id;
        UserId = userId != Guid.Empty ? userId : throw new ArgumentException("UserId is required", nameof(userId));
        DeliveryAddress = deliveryAddress ?? throw new ArgumentNullException(nameof(deliveryAddress));
        _items.AddRange(items);
        Metadata = metadata ?? new Dictionary<string, string>();

        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        OrderNumber = $"LVK-{CreatedAt:yyyyMMddHHmmssfff}-{Random.Shared.Next(1000, 9999)}";
        Status = OrderStatus.Initialized;

        _stageHistory.Add(new OrderStageHistory(Status, CreatedAt, "system", "Order initialized"));
    }

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; } = default!;
    public Guid UserId { get; private set; }
    public Address DeliveryAddress { get; private set; } = default!;
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    public IReadOnlyCollection<OrderStageHistory> StageHistory => _stageHistory.AsReadOnly();
    public Dictionary<string, string> Metadata { get; private set; } = new();
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount => _items.Sum(i => i.Total);
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public void UpdateMetadata(string key, string value)
    {
        Metadata[key] = value;
        Touch();
    }

    public void ReplaceMetadata(Dictionary<string, string> metadata)
    {
        Metadata = metadata;
        Touch();
    }

    public void UpdateDeliveryAddress(Address address)
    {
        DeliveryAddress = address;
        Touch();
    }

    public void RegisterStageChange(OrderStatus newStatus, string? actor = null, string? notes = null)
    {
        if (Status == newStatus)
        {
            return;
        }

        Status = newStatus;
        Touch();
        _stageHistory.Add(new OrderStageHistory(newStatus, UpdatedAt, actor, notes));
    }

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static Order Restore(
        Guid id,
        string orderNumber,
        Guid userId,
        Address deliveryAddress,
        IEnumerable<OrderItem> items,
        Dictionary<string, string> metadata,
        OrderStatus status,
        IEnumerable<OrderStageHistory> stageHistory,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        var order = new Order
        {
            Id = id,
            OrderNumber = orderNumber,
            UserId = userId,
            DeliveryAddress = deliveryAddress,
            Metadata = metadata is null
                ? new Dictionary<string, string>()
                : new Dictionary<string, string>(metadata),
            Status = status,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        if (items is not null)
        {
            order._items.AddRange(items);
        }

        if (stageHistory is not null)
        {
            order._stageHistory.AddRange(stageHistory.OrderBy(s => s.ChangedAt));
        }

        return order;
    }
}

