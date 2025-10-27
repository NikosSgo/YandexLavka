using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class PickingTask
{
    public Guid TaskId { get; private set; }
    public Guid OrderId { get; private set; }
    public string AssignedPicker { get; private set; }
    public PickingTaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string Zone { get; private set; }

    private readonly List<PickingItem> _items = new();
    public IReadOnlyCollection<PickingItem> Items => _items.AsReadOnly();

    public PickingTask(Guid orderId, List<PickingItem> items, string zone, string assignedPicker = null)
    {
        TaskId = Guid.NewGuid();
        OrderId = orderId;
        AssignedPicker = assignedPicker;
        Status = PickingTaskStatus.Created;
        Zone = zone;
        CreatedAt = DateTime.UtcNow;
        _items = items;
    }

    public void StartPicking(string pickerId)
    {
        if (Status != PickingTaskStatus.Created)
            throw new DomainException("Picking already started or completed");

        Status = PickingTaskStatus.InProgress;
        AssignedPicker = pickerId;
    }

    public void Complete()
    {
        if (Status != PickingTaskStatus.InProgress)
            throw new DomainException("Picking not in progress");

        Status = PickingTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == PickingTaskStatus.Completed)
            throw new DomainException("Cannot cancel completed picking task");

        Status = PickingTaskStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
    }
}