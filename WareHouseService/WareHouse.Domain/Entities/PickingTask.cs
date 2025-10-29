using System.ComponentModel.DataAnnotations.Schema;
using WareHouse.Domain.Common;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class PickingTask : AggregateRoot
{
    public Guid TaskId { get; private set; }
    public Guid OrderId { get; private set; }
    public string AssignedPicker { get; private set; }
    public PickingTaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string Zone { get; private set; }

    private readonly List<PickingItem> _items = new();
    public IReadOnlyCollection<PickingItem> Items => _items.AsReadOnly();

    [NotMapped]
    public bool AllItemsPicked => _items.All(item => item.IsPicked);

    [NotMapped]
    public decimal Progress => _items.Count > 0 ?
        (decimal)_items.Count(item => item.IsPicked) / _items.Count * 100 : 0;

    [NotMapped]
    public int TotalItems => _items.Count;

    [NotMapped]
    public int PickedItemsCount => _items.Count(item => item.IsPicked);

    private PickingTask() { }

    public PickingTask(Guid orderId, List<PickingItem> items, string zone, string assignedPicker = null)
    {
        TaskId = Guid.NewGuid();
        OrderId = orderId;
        AssignedPicker = assignedPicker;
        Status = PickingTaskStatus.Created;
        Zone = zone;
        CreatedAt = DateTime.UtcNow;

        // Устанавливаем PickingTaskId для каждого элемента
        foreach (var item in items)
        {
            // Создаем новый PickingItem с правильным TaskId
            var pickingItem = new PickingItem(
                TaskId,
                item.ProductId,
                item.ProductName,
                item.Sku,
                item.Quantity,
                item.StorageLocation,
                item.Barcode
            );
            _items.Add(pickingItem);
        }
    }

    public void StartPicking(string pickerId)
    {
        if (Status != PickingTaskStatus.Created)
            throw new DomainException("Picking already started or completed");

        Status = PickingTaskStatus.InProgress;
        AssignedPicker = pickerId;
        StartedAt = DateTime.UtcNow;
        UpdateTimestamps();
    }

    public void Complete()
    {
        if (Status != PickingTaskStatus.InProgress)
            throw new DomainException("Picking not in progress");

        if (_items.Any(item => !item.IsPicked))
            throw new DomainException("Cannot complete task with unpicked items");

        Status = PickingTaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamps();
    }

    public void Cancel(string reason = null)
    {
        if (Status == PickingTaskStatus.Completed)
            throw new DomainException("Cannot cancel completed picking task");

        Status = PickingTaskStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        UpdateTimestamps();
    }

    public void UpdateItemPickedStatus(Guid productId, int quantityPicked)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException($"Product {productId} not found in picking task");

        item.MarkAsPicked(quantityPicked);
        UpdateTimestamps();
    }

}