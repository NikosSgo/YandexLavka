using MongoDB.Driver;
using OrderService.Application.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Persistence.Documents;
using OrderService.Infrastructure.Persistence.Mappers;

namespace OrderService.Infrastructure.Persistence.Repositories;

internal class MongoOrderRepository : IOrderRepository
{
    private readonly IMongoCollection<OrderDocument> _orders;

    public MongoOrderRepository(MongoContext context)
    {
        _orders = context.Orders;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var cursor = await _orders
            .FindAsync(o => o.Id == id, cancellationToken: cancellationToken);

        var document = await cursor.FirstOrDefaultAsync(cancellationToken);
        return OrderDocumentMapper.ToDomain(document);
    }

    public Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        var document = OrderDocumentMapper.ToDocument(order);
        return _orders.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public Task UpdateAsync(Order order, CancellationToken cancellationToken)
    {
        var document = OrderDocumentMapper.ToDocument(order);
        return _orders.ReplaceOneAsync(
            o => o.Id == order.Id,
            document,
            new ReplaceOptions { IsUpsert = false },
            cancellationToken);
    }
}

