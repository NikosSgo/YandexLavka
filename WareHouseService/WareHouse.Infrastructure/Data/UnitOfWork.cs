using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using WareHouse.Domain.Interfaces;
using WareHouse.Infrastructure.Data.Repositories;

namespace WareHouse.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly IServiceProvider _serviceProvider;
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;
    private bool _disposed = false;
    private bool _inTransaction = false;

    public UnitOfWork(IDatabaseConnectionFactory connectionFactory, ILogger<UnitOfWork> logger, IServiceProvider serviceProvider)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
        _serviceProvider = serviceProvider;

        // ✅ АВТОМАТИЧЕСКАЯ ИНИЦИАЛИЗАЦИЯ РЕПОЗИТОРИЕВ БЕЗ ТРАНЗАКЦИИ
        InitializeRepositoriesWithoutTransaction();
    }

    public IOrderRepository Orders { get; private set; } = null!;
    public IPickingTaskRepository PickingTasks { get; private set; } = null!;
    public IStorageUnitRepository StorageUnits { get; private set; } = null!;
    public IProductRepository Products { get; private set; } = null!;

    private void InitializeRepositoriesWithoutTransaction()
    {
        // Получаем логгеры из DI контейнера
        var productLogger = _serviceProvider.GetService<ILogger<ProductRepository>>() ??
                           Microsoft.Extensions.Logging.Abstractions.NullLogger<ProductRepository>.Instance;

        // ✅ СОЗДАЕМ РЕПОЗИТОРИИ БЕЗ ТРАНЗАКЦИИ ДЛЯ ЗАПРОСОВ
        Orders = new OrderRepository(_connectionFactory);
        PickingTasks = new PickingTaskRepository(_connectionFactory);
        StorageUnits = new StorageUnitRepository(_connectionFactory);
        Products = new ProductRepository(_connectionFactory, productLogger);
    }

    private void InitializeRepositoriesWithTransaction()
    {
        if (_connection == null || _transaction == null)
            throw new InvalidOperationException("Connection and transaction must be initialized first");

        // Получаем логгеры из DI контейнера
        var productLogger = _serviceProvider.GetService<ILogger<ProductRepository>>() ??
                           Microsoft.Extensions.Logging.Abstractions.NullLogger<ProductRepository>.Instance;

        // ✅ СОЗДАЕМ РЕПОЗИТОРИИ С ТРАНЗАКЦИЕЙ ДЛЯ КОМАНД
        Orders = new OrderRepository(_connection, _transaction);
        PickingTasks = new PickingTaskRepository(_connection, _transaction);
        StorageUnits = new StorageUnitRepository(_connection, _transaction);
        Products = new ProductRepository(_connection, _transaction, productLogger);
    }

    public async Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        if (_transaction != null)
            throw new InvalidOperationException("Transaction already started");

        _connection = await _connectionFactory.CreateConnectionAsync();
        _transaction = await _connection.BeginTransactionAsync(isolationLevel);
        _inTransaction = true;

        // ✅ ПЕРЕИНИЦИАЛИЗИРУЕМ РЕПОЗИТОРИИ С ТРАНЗАКЦИЕЙ
        InitializeRepositoriesWithTransaction();

        _logger.LogInformation("Transaction started successfully");
    }

    public async Task CommitAsync()
    {
        if (_transaction == null)
            throw new InvalidOperationException("No transaction to commit");

        try
        {
            await _transaction.CommitAsync();
            _logger.LogInformation("Transaction committed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error committing transaction");
            await RollbackAsync();
            throw;
        }
        finally
        {
            await CleanupTransactionAsync();
            // ✅ ВОЗВРАЩАЕМ РЕПОЗИТОРИИ В РЕЖИМ БЕЗ ТРАНЗАКЦИИ
            InitializeRepositoriesWithoutTransaction();
        }
    }

    public async Task RollbackAsync()
    {
        if (_transaction != null)
        {
            try
            {
                await _transaction.RollbackAsync();
                _logger.LogInformation("Transaction rolled back successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during transaction rollback");
            }
            finally
            {
                await CleanupTransactionAsync();
                // ✅ ВОЗВРАЩАЕМ РЕПОЗИТОРИИ В РЕЖИМ БЕЗ ТРАНЗАКЦИИ
                InitializeRepositoriesWithoutTransaction();
            }
        }
    }

    private async Task CleanupTransactionAsync()
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

        _inTransaction = false;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (!_inTransaction)
        {
            _logger.LogWarning("SaveChangesAsync called without active transaction - changes may not be persisted");
            return 1; // Для запросов просто возвращаем успех
        }

        if (_transaction == null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        try
        {
            await _transaction.CommitAsync();
            return 1;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes");
            await RollbackAsync();
            throw;
        }
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        if (!_inTransaction)
        {
            _logger.LogWarning("SaveEntitiesAsync called without active transaction - changes may not be persisted");
            return true; // Для запросов просто возвращаем успех
        }

        if (_transaction == null)
            throw new InvalidOperationException("No active transaction. Call BeginTransactionAsync first.");

        try
        {
            await _transaction.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving entities");
            await RollbackAsync();
            throw;
        }
    }

    private async Task CleanupAsync()
    {
        if (_inTransaction)
        {
            await CleanupTransactionAsync();
        }
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
            if (_inTransaction)
            {
                _transaction?.Dispose();
                _connection?.Dispose();
            }
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    ~UnitOfWork()
    {
        Dispose();
    }
}