using WareHouse.Domain.Entities;

namespace WareHouse.Application.DTOs;

public record PickingTaskDto
{
    public Guid TaskId { get; init; }
    public Guid OrderId { get; init; }
    public string AssignedPicker { get; init; }
    public string Status { get; init; }
    public string Zone { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public decimal Progress { get; init; }
    public List<PickingItemDto> Items { get; init; } = new();

    public static PickingTaskDto FromEntity(PickingTask task)
    {
        return new PickingTaskDto
        {
            TaskId = task.TaskId,
            OrderId = task.OrderId,
            AssignedPicker = task.AssignedPicker,
            Status = task.Status.ToString(),
            Zone = task.Zone,
            CreatedAt = task.CreatedAt,
            CompletedAt = task.CompletedAt,
            Progress = task.Progress,
            Items = task.Items.Select(PickingItemDto.FromEntity).ToList()
        };
    }
}

public record PickingItemDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; }
    public string Sku { get; init; }
    public int Quantity { get; init; }
    public int QuantityPicked { get; init; }
    public string StorageLocation { get; init; }
    public string Barcode { get; init; }
    public bool IsPicked { get; init; }

    public static PickingItemDto FromEntity(PickingItem item)
    {
        return new PickingItemDto
        {
            ProductId = item.ProductId,
            ProductName = item.ProductName,
            Sku = item.Sku,
            Quantity = item.Quantity,
            QuantityPicked = item.QuantityPicked,
            StorageLocation = item.StorageLocation,
            Barcode = item.Barcode,
            IsPicked = item.IsPicked
        };
    }
}