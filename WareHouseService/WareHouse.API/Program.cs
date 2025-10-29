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
    Console.WriteLine("🚀 STARTING APPLICATION ON PORT 5433 WITH PASSWORD");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ ИСПОЛЬЗУЕМ ПРАВИЛЬНЫЙ CONNECTION STRING С ПОРТОМ 5433 И ПАРОЛЕМ
    var connectionString = "Host=localhost;Port=5433;Database=WareHouseDb;Username=postgres;Password=password;";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

    Console.WriteLine($"🔧 CONNECTION: {connectionString.Replace("password", "****")}");

    // ✅ ТЕСТИРУЕМ ПОДКЛЮЧЕНИЕ С ПАРОЛЕМ И ПОРТОМ 5433
    Console.WriteLine("🔌 TESTING CONNECTION TO PORT 5433 WITH PASSWORD...");
    try
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        Console.WriteLine("✅ CONNECTION SUCCESS WITH PASSWORD!");

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
        Console.WriteLine($"❌ CONNECTION FAILED: {ex.Message}");
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

    // Initialize Database
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Console.WriteLine("🗄️ APPLYING DATABASE MIGRATIONS...");
        await context.Database.MigrateAsync();

        Console.WriteLine("🌱 SEEDING DATABASE...");
        await DatabaseSeeder.SeedAsync(context);

        Console.WriteLine("✅ DATABASE INITIALIZED SUCCESSFULLY");
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
        Console.WriteLine("📚 SWAGGER ENABLED");
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

    Console.WriteLine("🎉 APPLICATION STARTED SUCCESSFULLY!");
    Console.WriteLine("📍 API: https://localhost:7001/api-docs");

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"💥 ERROR: {ex.Message}");
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}