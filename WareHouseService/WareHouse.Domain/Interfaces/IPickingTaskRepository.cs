using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;

using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Interfaces;

public interface IPickingTaskRepository : IRepository<PickingTask>
{
    Task<PickingTask> GetByIdAsync(Guid taskId);
    Task<PickingTask> GetActiveForOrderAsync(Guid orderId);
    Task<List<PickingTask>> GetByPickerAsync(string pickerId);
    Task<List<PickingTask>> GetTasksByStatusAsync(PickingTaskStatus status);
    Task<List<PickingTask>> GetTasksByZoneAsync(string zone);
}