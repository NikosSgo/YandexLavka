using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;

namespace WareHouse.Domain.Interfaces;

public interface IOrderRepository
{
    Task<OrderAggregate> GetByIdAsync(Guid orderId);
    Task<List<OrderAggregate>> GetOrdersByStatusAsync(OrderStatus status);
    Task<bool> ExistsAsync(Guid orderId);
    Task<List<OrderAggregate>> GetOrdersByPickerAsync(string pickerId);
    Task<List<OrderAggregate>> GetOrdersCreatedAfterAsync(DateTime date);

    // IRepository methods
    Task AddAsync(OrderAggregate entity);
    Task UpdateAsync(OrderAggregate entity);
    Task DeleteAsync(OrderAggregate entity);
    Task<IReadOnlyList<OrderAggregate>> GetAllAsync();
}