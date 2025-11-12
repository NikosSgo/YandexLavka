//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Npgsql;
//using System.Data;
//using WareHouse.Domain.Interfaces;
//using WareHouse.Infrastructure.Data.Repositories;

//namespace WareHouse.Infrastructure.Data;

//public class QueryUnitOfWork : IUnitOfWork
//{
//    private readonly IDatabaseConnectionFactory _connectionFactory;
//    private readonly ILogger<QueryUnitOfWork> _logger;
//    private readonly IServiceProvider _serviceProvider;
//    private bool _disposed = false;

//    public QueryUnitOfWork(IDatabaseConnectionFactory connectionFactory, ILogger<QueryUnitOfWork> logger, IServiceProvider serviceProvider)
//    {
//        _connectionFactory = connectionFactory;
//        _logger = logger;
//        _serviceProvider = serviceProvider;

//        // Инициализируем репозитории без транзакции для запросов
//        InitializeRepositories();
//    }

//    public IOrderRepository Orders { get; private set; } = null!;
//    public IPickingTaskRepository PickingTasks { get; private set; } = null!;
//    public IStorageUnitRepository StorageUnits { get; private set; } = null!;
//    public IProductRepository Products { get; private set; } = null!;

//    private void InitializeRepositories()
//    {
//        // Получаем логгеры из DI контейнера
//        var productLogger = _serviceProvider.GetService<ILogger<ProductRepository>>() ??
//                           Microsoft.Extensions.Logging.Abstractions.NullLogger<ProductRepository>.Instance;

//        // Создаем репозитории без транзакции (только для чтения)
//        Orders = new OrderRepository(_connectionFactory);
//        PickingTasks = new PickingTaskRepository(_connectionFactory);
//        StorageUnits = new StorageUnitRepository(_connectionFactory);
//        Products = new ProductRepository(_connectionFactory, productLogger);
//    }

//    // Эти методы не нужны для запросов, но должны быть реализованы из интерфейса
//    public Task BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
//    {
//        throw new NotSupportedException("Transactions are not supported in QueryUnitOfWork");
//    }

//    public Task CommitAsync()
//    {
//        throw new NotSupportedException("Transactions are not supported in QueryUnitOfWork");
//    }

//    public Task RollbackAsync()
//    {
//        throw new NotSupportedException("Transactions are not supported in QueryUnitOfWork");
//    }

//    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
//    {
//        throw new NotSupportedException("SaveChanges is not supported in QueryUnitOfWork");
//    }

//    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
//    {
//        throw new NotSupportedException("SaveEntities is not supported in QueryUnitOfWork");
//    }

//    public async ValueTask DisposeAsync()
//    {
//        if (!_disposed)
//        {
//            _disposed = true;
//        }
//        GC.SuppressFinalize(this);
//    }

//    public void Dispose()
//    {
//        if (!_disposed)
//        {
//            _disposed = true;
//        }
//        GC.SuppressFinalize(this);
//    }
//}