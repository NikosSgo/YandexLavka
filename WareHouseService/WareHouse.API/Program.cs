using Serilog;
using WareHouse.API.Configuration;
using WareHouse.API.Middleware;

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

    // Configure Pipeline
    app.ConfigurePipeline();

    Log.Information("WareHouse API application started successfully");

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