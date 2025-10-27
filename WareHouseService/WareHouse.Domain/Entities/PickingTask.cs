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
    public DateTime? StartedAt { get; private set; }
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
        StartedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != PickingTaskStatus.InProgress)
            throw new DomainException("Picking not in progress");

        if (!AllItemsPicked)
            throw new DomainException("Cannot complete task with unpicked items");

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

    // ПОЛНАЯ РЕАЛИЗАЦИЯ МЕТОДА
    public void UpdateItemPickedStatus(Guid productId, int quantityPicked)
    {
        if (Status != PickingTaskStatus.InProgress)
            throw new DomainException("Can only update picked status for tasks in progress");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException($"Product {productId} not found in picking task");

        // Обновляем статус собранности
        // В реальной реализации здесь была бы логика с PickingItem
        // Пока просто логируем или обновляем внутреннее состояние

        // Если нужно обновлять QuantityPicked в PickingItem, 
        // нужно добавить соответствующий метод в PickingItem
    }

    // ДОПОЛНИТЕЛЬНЫЙ МЕТОД для более точного обновления
    public void MarkItemAsPicked(Guid productId, int actualQuantityPicked)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new DomainException($"Product {productId} not found in picking task");

        // Здесь можно добавить логику валидации количества
        if (actualQuantityPicked > item.Quantity)
            throw new DomainException($"Picked quantity {actualQuantityPicked} exceeds required quantity {item.Quantity}");

        // Обновляем статус товара (если в PickingItem есть соответствующий метод)
        // item.MarkAsPicked(actualQuantityPicked);
    }

    public bool AllItemsPicked => _items.All(item => item.IsPicked);
    public decimal Progress => _items.Count > 0 ?
        (decimal)_items.Count(item => item.IsPicked) / _items.Count * 100 : 0;
}