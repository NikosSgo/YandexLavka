using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;

namespace WareHouse.Domain.Interfaces;

public interface IOrderRepository : IRepository<OrderAggregate>
{
    Task<OrderAggregate?> GetByIdAsync(Guid id);
    Task<List<OrderAggregate>> GetByStatusAsync(OrderStatus status);
    Task<List<OrderLine>> GetOrderLinesAsync(Guid orderId);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status);
}