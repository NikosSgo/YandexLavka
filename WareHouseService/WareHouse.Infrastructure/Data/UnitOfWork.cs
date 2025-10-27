using WareHouse.Domain.Interfaces;
using WareHouse.Infrastructure.Data.Repositories;

namespace WareHouse.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(
        ApplicationDbContext context,
        IOrderRepository orders,
        IPickingTaskRepository pickingTasks,
        IStorageUnitRepository storageUnits)
    {
        _context = context;
        Orders = orders;
        PickingTasks = pickingTasks;
        StorageUnits = storageUnits;
    }

    public IOrderRepository Orders { get; }
    public IPickingTaskRepository PickingTasks { get; }
    public IStorageUnitRepository StorageUnits { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}