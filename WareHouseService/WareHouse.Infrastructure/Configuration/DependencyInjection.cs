using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.Domain.Interfaces;
using WareHouse.Infrastructure.Data;
using WareHouse.Infrastructure.Data.Repositories;
using WareHouse.Infrastructure.Messaging;
using WareHouse.Infrastructure.Services;

namespace WareHouse.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database Connection Factory
        services.AddSingleton<IDatabaseConnectionFactory>(provider =>
            new DatabaseConnectionFactory(configuration.GetConnectionString("DefaultConnection")));

        // Unit of Work (теперь работает для запросов и команд)
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Репозитории для прямого использования (опционально)
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPickingTaskRepository, PickingTaskRepository>();
        services.AddScoped<IStorageUnitRepository, StorageUnitRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();

        // Services
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
        services.AddHostedService<KafkaConsumerService>();

        return services;
    }
}