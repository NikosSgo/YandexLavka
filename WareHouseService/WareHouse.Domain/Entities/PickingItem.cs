using System;

namespace WareHouse.Domain.Entities;

public class PickingItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public int QuantityPicked { get; private set; }
    public string StorageLocation { get; private set; }
    public string Barcode { get; private set; }
    public bool IsPicked => QuantityPicked >= Quantity;

    public PickingItem(Guid productId, string productName, string sku, int quantity, string storageLocation, string barcode)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Quantity = quantity;
        StorageLocation = storageLocation;
        Barcode = barcode;
        QuantityPicked = 0;
    }

    // ДОБАВЛЯЕМ МЕТОД ДЛЯ ОБНОВЛЕНИЯ СТАТУСА
    public void MarkAsPicked(int quantityPicked)
    {
        if (quantityPicked < 0 || quantityPicked > Quantity)
            throw new ArgumentException($"Invalid quantity picked: {quantityPicked}");

        QuantityPicked = quantityPicked;
    }
}