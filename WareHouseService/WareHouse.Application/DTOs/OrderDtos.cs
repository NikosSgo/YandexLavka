using WareHouse.Domain.Entities;

namespace WareHouse.Application.DTOs;

public record OrderDto
{
    public Guid OrderId { get; init; }
    public string CustomerId { get; init; }
    public string Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? PickingStartedAt { get; init; }
    public DateTime? PickingCompletedAt { get; init; }
    public List<OrderLineDto> Lines { get; init; } = new();
    public decimal TotalAmount { get; init; }

    public static OrderDto FromEntity(OrderAggregate order)
    {
        return new OrderDto
        {
            OrderId = order.OrderId,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            PickingStartedAt = order.PickingStartedAt,
            PickingCompletedAt = order.PickingCompletedAt,
            Lines = order.Lines.Select(OrderLineDto.FromEntity).ToList(),
            TotalAmount = order.Lines.Sum(line => line.TotalPrice)
        };
    }
}

public record OrderLineDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; }
    public string Sku { get; init; }
    public int QuantityOrdered { get; init; }
    public int QuantityPicked { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal TotalPrice { get; init; }
    public bool IsFullyPicked { get; init; }

    public static OrderLineDto FromEntity(OrderLine line)
    {
        return new OrderLineDto
        {
            ProductId = line.ProductId,
            ProductName = line.ProductName,
            Sku = line.Sku,
            QuantityOrdered = line.QuantityOrdered,
            QuantityPicked = line.QuantityPicked,
            UnitPrice = line.UnitPrice,
            TotalPrice = line.TotalPrice,
            IsFullyPicked = line.IsFullyPicked
        };
    }
}