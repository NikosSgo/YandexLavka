using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WareHouse.Infrastructure.Messaging;

public interface IKafkaProducerService
{
    Task ProduceAsync(string topic, object message);
    Task ProduceAsync(string topic, string key, object message);
}

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"],
            ClientId = "warehouse-service",
            Acks = Acks.All,
            MessageSendMaxRetries = 3,
            RetryBackoffMs = 1000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger = logger;
    }

    public async Task ProduceAsync(string topic, object message)
    {
        await ProduceAsync(topic, null, message);
    }

    public async Task ProduceAsync(string topic, string key, object message)
    {
        try
        {
            var jsonMessage = JsonSerializer.Serialize(message, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var kafkaMessage = new Message<string, string>
            {
                Key = key,
                Value = jsonMessage,
                Headers = new Headers
                {
                    { "event-type", System.Text.Encoding.UTF8.GetBytes(message.GetType().Name) },
                    { "produced-at", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                }
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage);

            _logger.LogInformation("Message delivered to {Topic} [{Partition}] at offset {Offset}",
                result.Topic, result.Partition, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Failed to deliver message to Kafka. Error: {Error}", ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}