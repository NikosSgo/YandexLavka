using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class OrderLine
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public string Sku { get; private set; }
    public int QuantityOrdered { get; private set; }
    public int QuantityPicked { get; private set; }
    public decimal UnitPrice { get; private set; }

    public OrderLine(Guid productId, string productName, string sku, int quantityOrdered, decimal unitPrice)
    {
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
    }

    public decimal TotalPrice => UnitPrice * QuantityOrdered;
    public bool IsFullyPicked => QuantityPicked >= QuantityOrdered;
}