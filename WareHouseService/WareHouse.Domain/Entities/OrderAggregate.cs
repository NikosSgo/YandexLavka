using System.ComponentModel.DataAnnotations.Schema;
using WareHouse.Domain.Common;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Events;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class OrderAggregate : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PickingStartedAt { get; private set; }
    public DateTime? PickingCompletedAt { get; private set; }
    public DateTime? PackingCompletedAt { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    // Вычисляемые свойства - должны быть помечены как [NotMapped]
    [NotMapped]
    public bool AllItemsPicked => _lines.All(line => line.QuantityPicked >= line.QuantityOrdered);

    [NotMapped]
    public decimal TotalAmount => _lines.Sum(line => line.TotalPrice);

    [NotMapped]
    public int TotalItems => _lines.Sum(line => line.QuantityOrdered);

    [NotMapped]
    public int PickedItems => _lines.Sum(line => line.QuantityPicked);

    private OrderAggregate() { }

    public OrderAggregate(Guid orderId, string customerId, List<OrderLine> lines)
    {
        OrderId = orderId;
        CustomerId = customerId;
        _lines = lines;
        Status = OrderStatus.Received;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new OrderReceivedEvent(OrderId, CustomerId, lines));
    }

    public void StartPicking()
    {
        if (Status != OrderStatus.Received)
            throw new DomainException($"Cannot start picking for order in {Status} status");

        Status = OrderStatus.Picking;
        PickingStartedAt = DateTime.UtcNow;
        UpdateTimestamps();

        AddDomainEvent(new OrderPickingStartedEvent(OrderId));
    }

    public void CompletePicking(Dictionary<Guid, int> pickedQuantities)
    {
        if (Status != OrderStatus.Picking)
            throw new DomainException($"Cannot complete picking for order in {Status} status");

        ValidatePickedQuantities(pickedQuantities);
        UpdatePickedQuantities(pickedQuantities);

        Status = OrderStatus.Picked;
        PickingCompletedAt = DateTime.UtcNow;
        UpdateTimestamps();

        AddDomainEvent(new OrderPickedEvent(OrderId, pickedQuantities));
    }

    public void CompletePacking()
    {
        if (Status != OrderStatus.Picked)
            throw new DomainException($"Cannot complete packing for order in {Status} status");

        Status = OrderStatus.Packed;
        PackingCompletedAt = DateTime.UtcNow;
        UpdateTimestamps();

        AddDomainEvent(new OrderPackedEvent(OrderId));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Packed || Status == OrderStatus.Cancelled)
            throw new DomainException($"Cannot cancel order in {Status} status");

        Status = OrderStatus.Cancelled;
        UpdateTimestamps();

        AddDomainEvent(new OrderCancelledEvent(OrderId, reason));
    }

    private void ValidatePickedQuantities(Dictionary<Guid, int> pickedQuantities)
    {
        foreach (var line in _lines)
        {
            if (!pickedQuantities.TryGetValue(line.ProductId, out var pickedQty))
                throw new DomainException($"Product {line.ProductId} not picked");

            if (pickedQty < line.QuantityOrdered)
                throw new DomainException($"Insufficient quantity for product {line.ProductId}. Ordered: {line.QuantityOrdered}, Picked: {pickedQty}");
        }
    }

    private void UpdatePickedQuantities(Dictionary<Guid, int> pickedQuantities)
    {
        foreach (var line in _lines)
        {
            if (pickedQuantities.TryGetValue(line.ProductId, out var pickedQty))
            {
                line.UpdatePickedQuantity(pickedQty);
            }
        }
    }
}