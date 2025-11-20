using Microsoft.Extensions.DependencyInjection;
using OrderService.Application.Abstractions;
using OrderService.Application.Services;

namespace OrderService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IOrderApplicationService, OrderApplicationService>();
        services.AddSingleton<IOrderStateMachine, OrderStateMachine>();

        return services;
    }
}

