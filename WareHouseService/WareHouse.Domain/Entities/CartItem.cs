using WareHouse.Domain.Common;
using WareHouse.Domain.Exceptions;

namespace WareHouse.Domain.Entities;

public class CartItem : Entity
{
    public string CustomerId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    // Конструктор для Dapper
    public CartItem()
    {
        CustomerId = string.Empty;
    }

    public CartItem(Guid id, string customerId, Guid productId, int quantity) : base(id)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new ArgumentNullException(nameof(customerId));
        
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId cannot be empty", nameof(productId));
        
        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        CustomerId = customerId;
        ProductId = productId;
        Quantity = quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new DomainException("Quantity must be greater than zero");

        Quantity = newQuantity;
        UpdateTimestamps();
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        Quantity += amount;
        UpdateTimestamps();
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Amount must be greater than zero");

        if (Quantity - amount <= 0)
            throw new DomainException("Quantity cannot be zero or negative after decrease");

        Quantity -= amount;
        UpdateTimestamps();
    }
}

