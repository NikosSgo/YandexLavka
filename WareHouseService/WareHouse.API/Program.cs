using Microsoft.EntityFrameworkCore;
using Serilog;
using WareHouse.API.Configuration;
using WareHouse.API.Middleware;
using WareHouse.Infrastructure.Data;

// Создаем логгер для начальной настройки
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting WareHouse API application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

    // Configuration
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    // Initialize Database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Применяем миграции
        await context.Database.MigrateAsync();

        // Заполняем тестовыми данными
        await DatabaseSeeder.SeedAsync(context);

        Log.Information("Database initialized successfully");
    }

    // Configure Pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "WareHouse API v1");
            c.RoutePrefix = "api-docs";
        });

        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // Global Exception Handling
    app.UseGlobalExceptionHandler();

    // Custom Middleware
    app.UseRequestLogging();
    app.UseCorrelationId();

    app.UseRouting();
    app.UseAuthorization();

    // Health Checks
    app.MapHealthChecks("/health");

    // API Controllers
    app.MapControllers();

    Log.Information("WareHouse API application started successfully");
    Log.Information("API Documentation: https://localhost:7001/api-docs");
    Log.Information("Health Checks: https://localhost:7001/health");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}