using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WareHouse.Infrastructure.Messaging;

public interface IKafkaProducerService
{
    Task ProduceAsync(string topic, string message);
}

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly ILogger<KafkaProducerService> _logger;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            MessageTimeoutMs = 5000,
            RequestTimeoutMs = 3000,
            Acks = Acks.Leader
        };

        _producer = new ProducerBuilder<Null, string>(config)
            .SetErrorHandler((_, error) =>
            {
                if (error.IsFatal)
                {
                    _logger.LogError("Fatal Kafka producer error: {Error}", error.Reason);
                }
                else
                {
                    _logger.LogWarning("Kafka producer error: {Error}", error.Reason);
                }
            })
            .Build();
    }

    public async Task ProduceAsync(string topic, string message)
    {
        try
        {
            var result = await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message });
            _logger.LogInformation("Message delivered to {Topic} [{Partition}] at offset {Offset}",
                result.Topic, result.Partition, result.Offset);
        }
        catch (ProduceException<Null, string> ex)
        {
            _logger.LogError(ex, "Failed to deliver message to topic {Topic}: {Error}", topic, ex.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(5));
        _producer?.Dispose();
    }
}