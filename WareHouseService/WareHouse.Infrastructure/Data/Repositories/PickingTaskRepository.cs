using Microsoft.EntityFrameworkCore;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class PickingTaskRepository : BaseRepository<PickingTask>, IPickingTaskRepository
{
    public PickingTaskRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async override Task<PickingTask> GetByIdAsync(Guid taskId)
    {
        return await _context.PickingTasks
            .Include(pt => pt.Items)
            .FirstOrDefaultAsync(pt => pt.TaskId == taskId);
    }

    public async Task<PickingTask> GetActiveForOrderAsync(Guid orderId)
    {
        return await _context.PickingTasks
            .Include(pt => pt.Items)
            .Where(pt => pt.OrderId == orderId &&
                        (pt.Status == PickingTaskStatus.Created || pt.Status == PickingTaskStatus.InProgress))
            .FirstOrDefaultAsync();
    }

    public async Task<List<PickingTask>> GetByPickerAsync(string pickerId)
    {
        return await _context.PickingTasks
            .Include(pt => pt.Items)
            .Where(pt => pt.AssignedPicker == pickerId)
            .OrderByDescending(pt => pt.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PickingTask>> GetTasksByStatusAsync(PickingTaskStatus status)
    {
        return await _context.PickingTasks
            .Include(pt => pt.Items)
            .Where(pt => pt.Status == status)
            .OrderBy(pt => pt.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<PickingTask>> GetTasksByZoneAsync(string zone)
    {
        return await _context.PickingTasks
            .Include(pt => pt.Items)
            .Where(pt => pt.Zone == zone &&
                        (pt.Status == PickingTaskStatus.Created || pt.Status == PickingTaskStatus.InProgress))
            .OrderBy(pt => pt.CreatedAt)
            .ToListAsync();
    }
}