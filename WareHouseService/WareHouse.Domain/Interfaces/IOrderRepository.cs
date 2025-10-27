using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;

using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Interfaces;

public interface IOrderRepository : IRepository<OrderAggregate>
{
    Task<OrderAggregate> GetByIdAsync(Guid orderId);
    Task<List<OrderAggregate>> GetOrdersByStatusAsync(OrderStatus status);
    Task<bool> ExistsAsync(Guid orderId);
    Task<List<OrderAggregate>> GetOrdersByPickerAsync(string pickerId);
    Task<List<OrderAggregate>> GetOrdersCreatedAfterAsync(DateTime date);
}
