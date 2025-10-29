// WareHouse.Infrastructure/Data/UnitOfWork.cs
using System.Data;
using Npgsql;
using WareHouse.Domain.Interfaces;
using WareHouse.Infrastructure.Data.Repositories;

namespace WareHouse.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private NpgsqlConnection _connection;
    private NpgsqlTransaction _transaction;
    private bool _disposed = false;

    public UnitOfWork(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IOrderRepository Orders { get; private set; }
    public IPickingTaskRepository PickingTasks { get; private set; }
    public IStorageUnitRepository StorageUnits { get; private set; }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started");

        _connection = await _connectionFactory.CreateConnectionAsync();
        _transaction = await _connection.BeginTransactionAsync(isolationLevel);

        // Создаем репозитории с общей транзакцией
        Orders = new OrderRepository(_connection, _transaction);
        PickingTasks = new PickingTaskRepository(_connection, _transaction);
        StorageUnits = new StorageUnitRepository(_connection, _transaction);
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction to commit");

        try
        {
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
        finally
        {
            await CleanupAsync();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            try
            {
                await _transaction.RollbackAsync();
            }
            catch
            {
                // Логируем ошибку отката, но не выбрасываем исключение
                Console.WriteLine("Error during transaction rollback");
            }
            finally
            {
                await CleanupAsync();
            }
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        try
        {
            await _transaction.CommitAsync();
            return 1; // В Dapper нет точного аналога, возвращаем 1 для успеха
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        try
        {
            await _transaction.CommitAsync();
            return true;
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    private async Task CleanupAsync()
    {
        if (_transaction != null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
            _connection = null;
        }

        // Сбрасываем репозитории
        Orders = null;
        PickingTasks = null;
        StorageUnits = null;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await CleanupAsync();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _transaction?.Dispose();
            _connection?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~UnitOfWork()
    {
        Dispose();
    }
}