using WareHouse.Domain.Common;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class OrderLine : Entity  // ← Наследуем от Entity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int QuantityOrdered { get; private set; }
    public int QuantityPicked { get; private set; }
    public decimal UnitPrice { get; private set; }

    // Навигационное свойство
    public OrderAggregate Order { get; private set; }

    private OrderLine() { }

    public OrderLine(Guid id, Guid orderId, Guid productId, string productName, string sku,
                    int quantityOrdered, decimal unitPrice) : base(id)
    {
        OrderId = orderId;
        ProductId = productId;
        ProductName = productName;
        Sku = sku;
        QuantityOrdered = quantityOrdered;
        UnitPrice = unitPrice;
        QuantityPicked = 0;
    }

    public void UpdatePickedQuantity(int quantity)
    {
        if (quantity > QuantityOrdered)
            throw new DomainException($"Picked quantity cannot exceed ordered quantity");

        QuantityPicked = quantity;
        UpdateTimestamps();
    }

    public decimal TotalPrice => UnitPrice * QuantityOrdered;
    public bool IsFullyPicked => QuantityPicked >= QuantityOrdered;
}