using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WareHouse.Infrastructure.Messaging;

public class KafkaConsumerService : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KafkaConsumerService> _logger;
    private readonly string[] _topics;

    public KafkaConsumerService(
        IConfiguration configuration,
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumerService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            GroupId = "warehouse-service-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _topics = new[] { "orders", "warehouse-commands" };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topics);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = _consumer.Consume(stoppingToken);
                await ProcessMessageAsync(consumeResult);
                _consumer.StoreOffset(consumeResult);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Error consuming message from Kafka");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consumer stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Kafka consumer");
            }
        }

        _consumer.Close();
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> consumeResult)
    {
        using var scope = _serviceProvider.CreateScope();

        try
        {
            var eventTypeHeader = consumeResult.Message.Headers.FirstOrDefault(h => h.Key == "event-type");
            if (eventTypeHeader == null)
            {
                _logger.LogWarning("Message without event-type header received");
                return;
            }

            var eventType = System.Text.Encoding.UTF8.GetString(eventTypeHeader.GetValueBytes());
            await HandleEventAsync(eventType, consumeResult.Message.Value, scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Kafka message from topic {Topic}", consumeResult.Topic);
        }
    }

    private async Task HandleEventAsync(string eventType, string message, IServiceProvider serviceProvider)
    {
        // Здесь будет логика обработки различных типов событий
        // Например: OrderCreatedEvent, OrderCancelledEvent и т.д.
        _logger.LogInformation("Processing event {EventType}: {Message}", eventType, message);

        // Пример обработки:
        switch (eventType)
        {
            case "OrderCreatedEvent":
                var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(message);
                // Обработка создания заказа
                break;
            case "OrderCancelledEvent":
                var orderCancelled = JsonSerializer.Deserialize<OrderCancelledEvent>(message);
                // Обработка отмены заказа
                break;
            default:
                _logger.LogWarning("Unknown event type: {EventType}", eventType);
                break;
        }

        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        _consumer?.Close();
        _consumer?.Dispose();
        base.Dispose();
    }
}

// DTO для событий Kafka
public class OrderCreatedEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; }
    public List<OrderLineDto> Items { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public string Reason { get; set; }
    public DateTime CancelledAt { get; set; }
}

public class OrderLineDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}