using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Common;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class StorageUnit : AggregateRoot
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int Quantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public string Location { get; private set; }
    public string Zone { get; private set; }
    public DateTime LastRestocked { get; private set; }

    public int AvailableQuantity => Quantity - ReservedQuantity;

    public bool IsLowStock { get; private set; }
    public bool IsOutOfStock { get; private set; }

    // Конструктор для Dapper
    public StorageUnit()
    {
        // Инициализация для Dapper
        ProductName = string.Empty;
        Sku = string.Empty;
        Location = string.Empty;
        Zone = string.Empty;
    }

    public StorageUnit(Guid productId, string productName, string sku, int quantity, string location, string zone)
    {
        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentNullException(nameof(productName));
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentNullException(nameof(sku));
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentNullException(nameof(location));
        if (string.IsNullOrWhiteSpace(zone))
            throw new ArgumentNullException(nameof(zone));

        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        Quantity = quantity;
        Location = location;
        Zone = zone;
        ReservedQuantity = 0;
        LastRestocked = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdateStockFlags();
    }

    // Методы для управления запасом
    public void Reserve(int quantity)
    {
        if (AvailableQuantity < quantity)
            throw new DomainException($"Insufficient available quantity. Available: {AvailableQuantity}, Requested: {quantity}");

        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservation(int quantity)
    {
        if (ReservedQuantity < quantity)
            throw new DomainException($"Cannot release more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Pick(int quantity)
    {
        if (ReservedQuantity < quantity)
            throw new DomainException($"Cannot pick more than reserved. Reserved: {ReservedQuantity}, Requested: {quantity}");

        if (Quantity < quantity)
            throw new DomainException($"Insufficient physical quantity. Available: {Quantity}, Requested: {quantity}");

        Quantity -= quantity;
        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
        UpdateStockFlags();
    }

    public void Restock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Restock quantity must be positive");

        Quantity += quantity;
        LastRestocked = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        UpdateStockFlags();
    }

    // Internal методы для репозитория
    internal void SetReservedQuantity(int quantity)
    {
        ReservedQuantity = quantity;
    }

    internal void SetLastRestocked(DateTime date)
    {
        LastRestocked = date;
    }

    internal void SetStockFlags(bool isLowStock, bool isOutOfStock)
    {
        IsLowStock = isLowStock;
        IsOutOfStock = isOutOfStock;
    }

    // Public метод для обновления флагов запаса
    public void UpdateStockFlags()
    {
        IsLowStock = AvailableQuantity <= 10;
        IsOutOfStock = AvailableQuantity == 0;
    }

    // Public метод для установки количества (для adjust операции)
    public void SetQuantity(int newQuantity)
    {
        if (newQuantity < 0)
            throw new DomainException("Quantity cannot be negative");

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
        UpdateStockFlags();
    }
}