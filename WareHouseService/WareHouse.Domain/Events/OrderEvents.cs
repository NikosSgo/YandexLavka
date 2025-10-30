using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Events;

public abstract class DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

public class OrderReceivedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string CustomerId { get; }
    public List<OrderLine> Lines { get; }

    public OrderReceivedEvent(Guid orderId, string customerId, List<OrderLine> lines)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Lines = lines;
    }
}

public class OrderPickingStartedEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderPickingStartedEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class OrderPickedEvent : DomainEvent
{
    public Guid OrderId { get; }
    public Dictionary<Guid, int> PickedQuantities { get; }

    public OrderPickedEvent(Guid orderId, Dictionary<Guid, int> pickedQuantities)
    {
        OrderId = orderId;
        PickedQuantities = pickedQuantities;
    }
}

// ДОБАВВЛЕН ЭТОТ КЛАСС
public class OrderCompletedEvent : DomainEvent
{
    public Guid OrderId { get; }

    public OrderCompletedEvent(Guid orderId)
    {
        OrderId = orderId;
    }
}

public class OrderCancelledEvent : DomainEvent
{
    public Guid OrderId { get; }
    public string Reason { get; }

    public OrderCancelledEvent(Guid orderId, string reason)
    {
        OrderId = orderId;
        Reason = reason;
    }
}