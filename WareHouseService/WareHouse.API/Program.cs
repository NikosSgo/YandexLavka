using Microsoft.EntityFrameworkCore;
using Serilog;
using WareHouse.API.Configuration;
using WareHouse.API.Middleware;
using WareHouse.Infrastructure.Data;
using Npgsql;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Console.WriteLine("🚀 STARTING WAREHOUSE APPLICATION");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ ИСПОЛЬЗУЕМ ПРАВИЛЬНЫЙ CONNECTION STRING С ПОРТОМ 5433 И ПАРОЛЕМ
    var connectionString = "Host=localhost;Port=5433;Database=WareHouseDb;Username=postgres;Password=password;";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

    Console.WriteLine($"🔧 Database: WareHouseDb (localhost:5433)");

    // ✅ ТЕСТИРУЕМ ПОДКЛЮЧЕНИЕ К POSTGRESQL
    Console.WriteLine("🔌 TESTING DATABASE CONNECTION...");
    try
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        Console.WriteLine("✅ DATABASE CONNECTION SUCCESS!");

        var cmd = new NpgsqlCommand("SELECT current_database(), current_user, inet_server_port()", connection);
        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            Console.WriteLine($"📊 Database: {reader.GetString(0)}");
            Console.WriteLine($"👤 User: {reader.GetString(1)}");
            Console.WriteLine($"🔌 Port: {reader.GetInt32(2)}");
        }
        await connection.CloseAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ DATABASE CONNECTION FAILED: {ex.Message}");
        throw;
    }

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

    // ✅ ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ (ТОЛЬКО SEEDING)
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("🌱 SEEDING DATABASE WITH TEST DATA...");
        try
        {
            await DatabaseSeeder.SeedAsync(context);
            Console.WriteLine("✅ DATABASE SEEDED SUCCESSFULLY");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ SEEDING WARNING: {ex.Message}");
            Console.WriteLine("📝 Continuing application startup...");
        }
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
        Console.WriteLine("📚 SWAGGER ENABLED AT /api-docs");
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseGlobalExceptionHandler();
    app.UseRequestLogging();
    app.UseCorrelationId();
    app.UseRouting();
    app.UseAuthorization();

    app.MapHealthChecks("/health");
    app.MapControllers();

    Console.WriteLine("🎉 WAREHOUSE APPLICATION STARTED SUCCESSFULLY!");
    Console.WriteLine("📍 API Documentation: https://localhost:7001/api-docs");
    Console.WriteLine("📍 Health Checks: https://localhost:7001/health");
    Console.WriteLine("📍 PostgreSQL: localhost:5433");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"💥 APPLICATION STARTUP FAILED: {ex.Message}");
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}