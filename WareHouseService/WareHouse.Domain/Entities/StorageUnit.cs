using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class StorageUnit
{
    public Guid UnitId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public string Location { get; private set; }
    public string Zone { get; private set; }
    public DateTime LastRestocked { get; private set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;

    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }

    public StorageUnit(Guid productId, string productName, string sku, int quantity, string location, string zone)
    {
        UnitId = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Quantity = quantity;
        Location = location;
        Zone = zone;
        LastRestocked = DateTime.UtcNow;
    }

    public void Reserve(int quantity)
    {
        if (AvailableQuantity < quantity)
            throw new DomainException($"Insufficient available quantity. Available: {AvailableQuantity}, Requested: {quantity}");

        ReservedQuantity += quantity;
    }

    public void ReleaseReservation(int quantity)
    {
        if (ReservedQuantity < quantity)
            throw new DomainException($"Cannot release more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        ReservedQuantity -= quantity;
    }

    public void Pick(int quantity)
    {
        if (ReservedQuantity < quantity)
            throw new DomainException($"Cannot pick more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        if (Quantity < quantity)
            throw new DomainException($"Insufficient physical quantity. Available: {Quantity}, Requested: {quantity}");

        Quantity -= quantity;
        ReservedQuantity -= quantity;
    }

    public void Restock(int quantity)
    {
        Quantity += quantity;
        LastRestocked = DateTime.UtcNow;
    }
}