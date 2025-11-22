// WareHouse.Domain/Interfaces/IUnitOfWork.cs
using System.Data;

namespace WareHouse.Domain.Interfaces;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IOrderRepository Orders { get; }
    IPickingTaskRepository PickingTasks { get; }
    IStorageUnitRepository StorageUnits { get; }
    IProductRepository Products { get; }
    ICartRepository Cart { get; }

    Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    Task CommitAsync();
    Task RollbackAsync();
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);
}