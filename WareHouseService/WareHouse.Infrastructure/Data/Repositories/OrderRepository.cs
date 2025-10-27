using Microsoft.EntityFrameworkCore;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class OrderRepository : BaseRepository<OrderAggregate>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async override Task<OrderAggregate> GetByIdAsync(Guid orderId)
    {
        return await _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);
    }

    public async Task<List<OrderAggregate>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.Orders
            .Include(o => o.Lines)
            .Where(o => o.Status == status)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(Guid orderId)
    {
        return await _context.Orders.AnyAsync(o => o.OrderId == orderId);
    }

    public async Task<List<OrderAggregate>> GetOrdersByPickerAsync(string pickerId)
    {
        return await _context.Orders
            .Include(o => o.Lines)
            .Where(o => _context.PickingTasks
                .Any(pt => pt.OrderId == o.OrderId && pt.AssignedPicker == pickerId))
            .ToListAsync();
    }

    public async Task<List<OrderAggregate>> GetOrdersCreatedAfterAsync(DateTime date)
    {
        return await _context.Orders
            .Include(o => o.Lines)
            .Where(o => o.CreatedAt >= date)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}