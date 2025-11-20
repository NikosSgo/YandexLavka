using OrderService.Domain.Enums;

namespace OrderService.Domain.Entities;

public class OrderStageHistory
{
    public OrderStatus Status { get; private set; }
    public DateTimeOffset ChangedAt { get; private set; }
    public string? Actor { get; private set; }
    public string? Notes { get; private set; }

    private OrderStageHistory() { }

    public OrderStageHistory(OrderStatus status, DateTimeOffset changedAt, string? actor, string? notes)
    {
        Status = status;
        ChangedAt = changedAt;
        Actor = actor;
        Notes = notes;
    }
}

