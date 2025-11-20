using Microsoft.Extensions.Options;
using MongoDB.Driver;
using OrderService.Infrastructure.Options;
using OrderService.Infrastructure.Persistence.Documents;

namespace OrderService.Infrastructure.Persistence;

public class MongoContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoOptions _options;

    public MongoContext(IOptions<MongoOptions> options)
    {
        _options = options.Value;
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException("Mongo connection string is not configured");
        }

        var client = new MongoClient(_options.ConnectionString);
        _database = client.GetDatabase(_options.Database);
    }

    public IMongoCollection<OrderDocument> Orders =>
        _database.GetCollection<OrderDocument>(_options.OrdersCollection);
}

