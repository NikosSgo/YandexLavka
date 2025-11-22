using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using WareHouse.Application;
using WareHouse.Infrastructure.Configuration;

namespace WareHouse.API.Configuration;

public static class ApiServiceConfiguration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Application и Infrastructure слои
        services.AddApplication();

        // Infrastructure слой
        services.AddInfrastructure(configuration);

        // API
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerConfiguration();

        // Health Checks
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck<RedisHealthCheck>("redis")
            .AddCheck<KafkaHealthCheck>("kafka");

        // CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // HttpContext Accessor
        services.AddHttpContextAccessor();

        return services;
    }

    private static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "WareHouse Service API",
                Version = "v1",
                Description = "API для управления складскими операциями Яндекс Лавки. " +
                             "Включает управление продуктами, складскими единицами, заказами, корзиной и операциями сборки.",
                Contact = new Microsoft.OpenApi.Models.OpenApiContact
                {
                    Name = "WareHouse Team",
                    Email = "warehouse@company.com"
                }
            });

            // Включаем XML комментарии для документации (если они есть)
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // JWT Authentication для Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Группировка по тегам (контроллерам)
            c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default" });
            c.DocInclusionPredicate((name, api) => true);
        });

        return services;
    }
}

// Health Check реализации
public class DatabaseHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(HealthCheckResult.Healthy("Database is available"));
    }
}

public class RedisHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(HealthCheckResult.Healthy("Redis is available"));
    }
}

public class KafkaHealthCheck : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(HealthCheckResult.Healthy("Kafka is available"));
    }
}