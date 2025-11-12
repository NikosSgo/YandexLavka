namespace WareHouse.Application.DTOs;

public class OrderDto
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? PickingStartedAt { get; set; }
    public DateTime? PickingCompletedAt { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

public class OrderLineDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int QuantityOrdered { get; set; }
    public int QuantityPicked { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsFullyPicked { get; set; }
}