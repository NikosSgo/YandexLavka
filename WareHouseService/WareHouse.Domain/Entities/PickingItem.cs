using WareHouse.Domain.Common;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class PickingItem : Entity
{
    public Guid PickingTaskId { get; private set; } // ← Добавляем PickingTaskId
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public int QuantityPicked { get; private set; }
    public string StorageLocation { get; private set; }
    public string Barcode { get; private set; }
    public bool IsPicked { get; private set; }

    // ✅ ВЫЧИСЛЯЕМОЕ СВОЙСТВО - ДОЛЖНО РАБОТАТЬ КОРРЕКТНО
    //public bool IsPicked => QuantityPicked >= Quantity;

    // Навигационное свойство
    public PickingTask PickingTask { get; private set; }

    public PickingItem() { }

    public PickingItem(Guid pickingTaskId, Guid productId, string productName, string sku, int quantity,
        string storageLocation, string barcode, int quantityPicked = 0, bool isPicked = false)
    {
        PickingTaskId = pickingTaskId;
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Quantity = quantity;
        StorageLocation = storageLocation;
        Barcode = barcode;
        IsPicked = isPicked;
        QuantityPicked = quantityPicked;
    }

    public void MarkAsPicked(int quantityPicked)
    {
        if (quantityPicked < 0 || quantityPicked > Quantity)
            throw new DomainException($"Invalid quantity picked: {quantityPicked}");

        QuantityPicked = quantityPicked;
        IsPicked = quantityPicked >= Quantity;
        UpdateTimestamps();
    }
}