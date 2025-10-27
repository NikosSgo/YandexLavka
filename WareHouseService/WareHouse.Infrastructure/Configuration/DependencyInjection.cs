using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.Domain.Interfaces;
using WareHouse.Infrastructure.Behaviors; // Добавляем
using WareHouse.Infrastructure.Data;
using WareHouse.Infrastructure.Data.Repositories;
using WareHouse.Infrastructure.Messaging;
using WareHouse.Infrastructure.Services;

namespace WareHouse.Infrastructure.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPickingTaskRepository, PickingTaskRepository>();
        services.AddScoped<IStorageUnitRepository, StorageUnitRepository>();

        // Behaviors (если перенесли в Infrastructure)
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Services
        services.AddScoped<IDomainEventService, DomainEventService>();
        services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
        services.AddHostedService<KafkaConsumerService>();

        return services;
    }
}