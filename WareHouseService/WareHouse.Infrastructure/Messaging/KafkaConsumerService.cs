using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WareHouse.Infrastructure.Messaging;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string[] _topics;

    public KafkaConsumerService(IConfiguration configuration, ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:GroupId"] ?? "warehouse-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();

        // ✅ ИСПРАВЛЕНО: Берем топики из конфигурации, а не хардкод
        _topics = new[]
        {
            configuration["Kafka:Topics:Orders"] ?? "orders",
            configuration["Kafka:Topics:WarehouseCommands"] ?? "warehouse-commands",
            configuration["Kafka:Topics:WarehouseEvents"] ?? "warehouse-events",
            configuration["Kafka:Topics:PickingTasks"] ?? "picking-tasks",
            configuration["Kafka:Topics:StockUpdates"] ?? "stock-updates"
        };

        _logger.LogInformation("Kafka consumer configured for topics: {Topics}", string.Join(", ", _topics));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Kafka consumer for topics: {Topics}", string.Join(", ", _topics));

        // ✅ ДОБАВЬТЕ ЗАДЕРЖКУ ПЕРЕД ПОДКЛЮЧЕНИЕМ
        _logger.LogInformation("Waiting 10 seconds for Kafka to be ready...");
        await Task.Delay(10000, stoppingToken);

        try
        {
            _consumer.Subscribe(_topics);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(stoppingToken);

                    _logger.LogInformation("Received message from topic {Topic}: {Message}",
                        consumeResult.Topic, consumeResult.Message.Value);

                    await ProcessMessageAsync(consumeResult.Topic, consumeResult.Message.Value);
                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException ex) when (ex.Error.IsFatal)
                {
                    _logger.LogError(ex, "Fatal error consuming from Kafka. Stopping consumer.");
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogWarning(ex, "Error consuming message from Kafka. Retrying...");
                    await Task.Delay(1000, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer was cancelled");
                    break;
                }
            }
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _logger.LogInformation("Kafka consumer stopped");
        }
    }

    private async Task ProcessMessageAsync(string topic, string message)
    {
        try
        {
            _logger.LogInformation("Processing message from topic {Topic}: {Message}", topic, message);

            // Здесь будет логика обработки сообщений в зависимости от топика
            switch (topic)
            {
                case "orders":
                    await ProcessOrderMessage(message);
                    break;
                case "warehouse-commands":
                    await ProcessWarehouseCommand(message);
                    break;
                case "warehouse-events":
                    await ProcessWarehouseEvent(message);
                    break;
                case "picking-tasks":
                    await ProcessPickingTaskMessage(message);
                    break;
                case "stock-updates":
                    await ProcessStockUpdateMessage(message);
                    break;
                default:
                    _logger.LogWarning("Unknown topic received: {Topic}", topic);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from topic {Topic}", topic);
        }
    }

    private async Task ProcessOrderMessage(string message)
    {
        _logger.LogInformation("Processing order message: {Message}", message);
        await Task.CompletedTask;
    }

    private async Task ProcessWarehouseCommand(string message)
    {
        _logger.LogInformation("Processing warehouse command: {Message}", message);
        await Task.CompletedTask;
    }

    private async Task ProcessWarehouseEvent(string message)
    {
        _logger.LogInformation("Processing warehouse event: {Message}", message);
        await Task.CompletedTask;
    }

    private async Task ProcessPickingTaskMessage(string message)
    {
        _logger.LogInformation("Processing picking task: {Message}", message);
        await Task.CompletedTask;
    }

    private async Task ProcessStockUpdateMessage(string message)
    {
        _logger.LogInformation("Processing stock update: {Message}", message);
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}