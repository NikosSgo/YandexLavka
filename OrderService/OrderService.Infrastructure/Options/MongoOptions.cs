namespace OrderService.Infrastructure.Options;

public class MongoOptions
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = string.Empty;

    public string Database { get; set; } = "lavka-orders";

    public string OrdersCollection { get; set; } = "orders";
}

