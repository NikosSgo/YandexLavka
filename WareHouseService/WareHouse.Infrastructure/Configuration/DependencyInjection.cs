using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.Domain.Interfaces;
using WareHouse.Infrastructure.Data;
using WareHouse.Infrastructure.Data.Repositories;
using WareHouse.Infrastructure.Messaging;
using WareHouse.Infrastructure.Services;

namespace WareHouse.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPickingTaskRepository, PickingTaskRepository>();
        services.AddScoped<IStorageUnitRepository, StorageUnitRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
        services.AddHostedService<KafkaConsumerService>();

        // Caching
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        return services;
    }
}