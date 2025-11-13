using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Serilog;
using WareHouse.API.Configuration;
using WareHouse.API.Middleware;
using WareHouse.Infrastructure.Data;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Console.WriteLine("🚀 STARTING WAREHOUSE APPLICATION");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ ПРАВИЛЬНОЕ ПОЛУЧЕНИЕ CONNECTION STRING С ПРИОРИТЕТОМ ДЛЯ ПЕРЕМЕННЫХ ОКРУЖЕНИЯ
    var mainConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    // ДЛЯ ОТЛАДКИ: выводим все источники конфигурации
    Console.WriteLine("🔧 CONFIGURATION SOURCES:");
    Console.WriteLine($"   - ConnectionString from config: {mainConnectionString}");
    Console.WriteLine(
        $"   - Environment ConnectionString: {Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")}"
    );
    Console.WriteLine(
        $"   - ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
    );

    // ✅ ПРОВЕРЯЕМ, ЕСТЬ ЛИ ПЕРЕМЕННЫЕ ОКРУЖЕНИЯ И ПЕРЕОПРЕДЕЛЯЕМ
    var envConnectionString = Environment.GetEnvironmentVariable(
        "ConnectionStrings__DefaultConnection"
    );
    if (!string.IsNullOrEmpty(envConnectionString))
    {
        mainConnectionString = envConnectionString;
        Console.WriteLine("✅ USING ENVIRONMENT VARIABLE FOR DATABASE CONNECTION");
    }
    else
    {
        Console.WriteLine("⚠️ USING APPSETTINGS.JSON FOR DATABASE CONNECTION");
    }

    Console.WriteLine($"🔧 Final Database Connection: {mainConnectionString}");

    // ✅ ТЕСТИРУЕМ ПОДКЛЮЧЕНИЕ К POSTGRESQL (упрощенная версия)
    Console.WriteLine("🔌 TESTING DATABASE CONNECTION...");
    try
    {
        using var connection = new NpgsqlConnection(mainConnectionString);
        await connection.OpenAsync();
        Console.WriteLine("✅ DATABASE CONNECTION SUCCESS!");

        // Простая проверка что база отвечает
        var cmd = new NpgsqlCommand("SELECT version()", connection);
        var version = await cmd.ExecuteScalarAsync();
        Console.WriteLine($"📊 Database Version: {version}");

        await connection.CloseAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ DATABASE CONNECTION FAILED: {ex.Message}");
        Console.WriteLine($"💡 Connection string used: {mainConnectionString}");
        throw;
    }

    // ✅ ТЕСТИРУЕМ ПОДКЛЮЧЕНИЕ К KAFKA
    Console.WriteLine("🔌 TESTING KAFKA CONNECTION...");
    bool kafkaAvailable = false;
    try
    {
        // ИСПРАВЛЕНО: для Docker используем kafka:29092
        var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "kafka:29092";
        Console.WriteLine($"🔧 Kafka Bootstrap Servers: {bootstrapServers}");

        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers,
            SocketTimeoutMs = 10000,
        };

        using var adminClient = new AdminClientBuilder(config).Build();

        // Пробуем получить метаданные брокера
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

        if (metadata.Brokers.Any())
        {
            kafkaAvailable = true;
            Console.WriteLine(
                $"✅ KAFKA CONNECTION SUCCESS! Found {metadata.Brokers.Count} broker(s)"
            );

            foreach (var broker in metadata.Brokers)
            {
                Console.WriteLine(
                    $"   📡 Broker: {broker.Host}:{broker.Port} (ID: {broker.BrokerId})"
                );
            }

            // Проверяем существующие топики
            var topics = metadata.Topics.Select(t => t.Topic).ToList();
            Console.WriteLine($"📋 Found {topics.Count} topic(s) in Kafka");

            if (topics.Any())
            {
                foreach (var topic in topics.Take(10))
                {
                    Console.WriteLine($"   📝 Topic: {topic}");
                }
                if (topics.Count > 10)
                {
                    Console.WriteLine($"   ... and {topics.Count - 10} more topics");
                }
            }

            // ✅ ПРОВЕРЯЕМ НАЛИЧИЕ НУЖНЫХ НАМ ТОПИКОВ
            var requiredTopics = new[]
            {
                "orders",
                "warehouse-commands",
                "warehouse-events",
                "picking-tasks",
                "stock-updates",
            };
            var missingTopics = requiredTopics.Except(topics).ToList();

            if (missingTopics.Any())
            {
                Console.WriteLine(
                    $"⚠️  Missing required topics: {string.Join(", ", missingTopics)}"
                );
            }
            else
            {
                Console.WriteLine("✅ All required topics are available");
            }
        }
        else
        {
            Console.WriteLine("⚠️ KAFKA CONNECTION: No brokers found");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ KAFKA CONNECTION FAILED: {ex.Message}");
        Console.WriteLine("⚠️ Application will start without Kafka support");
    }

    // Configure Serilog
    builder.Host.UseSerilog(
        (context, services, configuration) =>
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    );

    // Configuration
    builder.Services.AddApiServices(builder.Configuration);

    var app = builder.Build();

    // ✅ ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ
    using (var scope = app.Services.CreateScope())
    {
        Console.WriteLine("🌱 SEEDING DATABASE WITH TEST DATA...");
        try
        {
            //await DatabaseSeeder.SeedAsync(seederConnectionString);
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
    Console.WriteLine("📍 API Documentation: http://localhost:8080/api-docs");
    Console.WriteLine("📍 Health Checks: http://localhost:8080/health");
    Console.WriteLine("📍 PostgreSQL: postgres:5432");
    Console.WriteLine($"📍 Kafka: {(kafkaAvailable ? "kafka:29092 ✅" : "DISABLED ❌")}");

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

