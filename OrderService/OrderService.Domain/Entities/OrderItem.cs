namespace OrderService.Domain.Entities;

public class OrderItem
{
    public string Sku { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public decimal Total => Price * Quantity;

    private OrderItem() { }

    public OrderItem(string sku, string name, decimal price, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU is required", nameof(sku));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }

        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Price must be positive");
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive");
        }

        Sku = sku;
        Name = name;
        Price = price;
        Quantity = quantity;
    }
}

