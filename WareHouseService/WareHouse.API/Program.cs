using Microsoft.EntityFrameworkCore;
using Serilog;
using WareHouse.API.Configuration;
using WareHouse.API.Middleware;
using WareHouse.Infrastructure.Data;
using Npgsql;
using Confluent.Kafka; // Добавляем using для Kafka

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Console.WriteLine("🚀 STARTING WAREHOUSE APPLICATION");

    var builder = WebApplication.CreateBuilder(args);

    // ✅ ИСПОЛЬЗУЕМ ПРАВИЛЬНЫЙ CONNECTION STRING С ПОРТОМ 5433 И ПАРОЛЕМ
    var mainConnectionString = "Host=localhost;Port=5433;Database=WareHouseDb;Username=postgres;Password=password;";
    builder.Configuration["ConnectionStrings:DefaultConnection"] = mainConnectionString;

    Console.WriteLine($"🔧 Database: WareHouseDb (localhost:5433)");

    // ✅ ТЕСТИРУЕМ ПОДКЛЮЧЕНИЕ К POSTGRESQL
    Console.WriteLine("🔌 TESTING DATABASE CONNECTION...");
    try
    {
        using var connection = new NpgsqlConnection(mainConnectionString);
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

    // ✅ ТЕСТИРУЕМ ПОДКЛЮЧЕНИЕ К KAFKA
    Console.WriteLine("🔌 TESTING KAFKA CONNECTION...");
    bool kafkaAvailable = false;
    try
    {
        var bootstrapServers = builder.Configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        Console.WriteLine($"🔧 Kafka Bootstrap Servers: {bootstrapServers}");

        var config = new AdminClientConfig
        {
            BootstrapServers = bootstrapServers,
            // ✅ ПРАВИЛЬНЫЕ СВОЙСТВА ДЛЯ AdminClientConfig
            SocketTimeoutMs = 10000
        };

        using var adminClient = new AdminClientBuilder(config).Build();

        // Пробуем получить метаданные брокера
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

        if (metadata.Brokers.Any())
        {
            kafkaAvailable = true;
            Console.WriteLine($"✅ KAFKA CONNECTION SUCCESS! Found {metadata.Brokers.Count} broker(s)");

            foreach (var broker in metadata.Brokers)
            {
                Console.WriteLine($"   📡 Broker: {broker.Host}:{broker.Port} (ID: {broker.BrokerId})");
            }

            // Проверяем существующие топики
            var topics = metadata.Topics.Select(t => t.Topic).ToList();
            Console.WriteLine($"📋 Found {topics.Count} topic(s) in Kafka");

            if (topics.Any())
            {
                foreach (var topic in topics.Take(10)) // Показываем первые 10 топиков
                {
                    Console.WriteLine($"   📝 Topic: {topic}");
                }
                if (topics.Count > 10)
                {
                    Console.WriteLine($"   ... and {topics.Count - 10} more topics");
                }
            }

            // ✅ ПРОВЕРЯЕМ НАЛИЧИЕ НУЖНЫХ НАМ ТОПИКОВ
            var requiredTopics = new[] { "orders", "warehouse-commands", "warehouse-events", "picking-tasks", "stock-updates" };
            var missingTopics = requiredTopics.Except(topics).ToList();

            if (missingTopics.Any())
            {
                Console.WriteLine($"⚠️  Missing required topics: {string.Join(", ", missingTopics)}");
                Console.WriteLine("💡 Consider creating these topics manually:");
                foreach (var topic in missingTopics)
                {
                    Console.WriteLine($"   docker exec -it warehouse-kafka kafka-topics --create --topic {topic} --bootstrap-server localhost:9092 --partitions 1 --replication-factor 1");
                }
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

        // Для отладки выводим больше информации об ошибке
        if (ex.InnerException != null)
        {
            Console.WriteLine($"   📖 Inner Exception: {ex.InnerException.Message}");
        }

        // Выводим возможные причины
        Console.WriteLine("💡 Possible solutions:");
        Console.WriteLine("   1. Ensure Kafka is running: docker ps | grep kafka");
        Console.WriteLine("   2. Check if Kafka is ready: docker logs warehouse-kafka");
        Console.WriteLine("   3. Wait a few seconds and restart the application");
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

    // ✅ ИНИЦИАЛИЗАЦИЯ БАЗЫ ДАННЫХ 
    using (var scope = app.Services.CreateScope())
    {
        var seederConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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
    Console.WriteLine("📍 API Documentation: https://localhost:7001/api-docs");
    Console.WriteLine("📍 Health Checks: https://localhost:7001/health");
    Console.WriteLine("📍 PostgreSQL: localhost:5433");
    Console.WriteLine($"📍 Kafka: {(kafkaAvailable ? "localhost:9092 ✅" : "DISABLED ❌")}");

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