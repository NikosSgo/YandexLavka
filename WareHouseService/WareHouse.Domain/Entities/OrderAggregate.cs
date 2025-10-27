using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Events;
using WareHouse.Domain.Exceptions;

using WareHouse.Domain.Events;

namespace WareHouse.Domain.Entities;

public class OrderAggregate
{
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string CustomerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? PickingStartedAt { get; private set; }
    public DateTime? PickingCompletedAt { get; private set; }

    private readonly List<OrderLine> _lines = new();
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private OrderAggregate() { }

    public OrderAggregate(Guid orderId, string customerId, List<OrderLine> lines)
    {
        OrderId = orderId;
        CustomerId = customerId;
        _lines = lines;
        Status = OrderStatus.Received;
        CreatedAt = DateTime.UtcNow;

        _domainEvents.Add(new OrderReceivedEvent(OrderId, CustomerId, lines));
    }

    public void StartPicking()
    {
        if (Status != OrderStatus.Received)
            throw new DomainException($"Cannot start picking for order in {Status} status");

        Status = OrderStatus.Picking;
        PickingStartedAt = DateTime.UtcNow;

        _domainEvents.Add(new OrderPickingStartedEvent(OrderId));
    }

    public void CompletePicking(Dictionary<Guid, int> pickedQuantities)
    {
        if (Status != OrderStatus.Picking)
            throw new DomainException($"Cannot complete picking for order in {Status} status");

        foreach (var line in _lines)
        {
            if (!pickedQuantities.TryGetValue(line.ProductId, out var pickedQty) || pickedQty < line.QuantityOrdered)
                throw new DomainException($"Insufficient quantity picked for product {line.ProductId}");
        }

        Status = OrderStatus.Picked;
        PickingCompletedAt = DateTime.UtcNow;

        _domainEvents.Add(new OrderPickedEvent(OrderId, pickedQuantities));
    }

    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Packed || Status == OrderStatus.Cancelled)
            throw new DomainException($"Cannot cancel order in {Status} status");

        Status = OrderStatus.Cancelled;

        _domainEvents.Add(new OrderCancelledEvent(OrderId, reason));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}