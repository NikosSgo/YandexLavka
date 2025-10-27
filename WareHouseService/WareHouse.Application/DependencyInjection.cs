using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WareHouse.Application.Behaviors;
using WareHouse.Application.Interfaces;
using WareHouse.Application.Services;

namespace WareHouse.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            // УБИРАЕМ TransactionBehavior отсюда
        });

        // Validators
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Services
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPickingService, PickingService>();
        services.AddScoped<IStockService, StockService>();

        return services;
    }
}