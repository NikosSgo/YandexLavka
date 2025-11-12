using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;

namespace WareHouse.Domain.Interfaces;

public interface IPickingTaskRepository
{
    Task<PickingTask> GetByIdAsync(Guid taskId);
    Task<PickingTask> GetActiveForOrderAsync(Guid orderId);
    Task<List<PickingTask>> GetByPickerAsync(string pickerId);
    Task<List<PickingTask>> GetTasksByStatusAsync(PickingTaskStatus status);
    Task<List<PickingTask>> GetTasksByZoneAsync(string zone);

    // IRepository methods
    Task AddAsync(PickingTask entity);
    Task UpdateAsync(PickingTask entity);
    Task DeleteAsync(PickingTask entity);
    Task<IReadOnlyList<PickingTask>> GetAllAsync();
}