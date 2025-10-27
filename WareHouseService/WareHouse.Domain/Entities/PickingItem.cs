using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WareHouse.Domain.Entities;

public class PickingItem
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public string StorageLocation { get; private set; }
    public string Barcode { get; private set; }

    public PickingItem(Guid productId, string productName, string sku, int quantity, string storageLocation, string barcode)
    {
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Quantity = quantity;
        StorageLocation = storageLocation;
        Barcode = barcode;
    }
}