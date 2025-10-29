using Dapper;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    // Конструктор для обычного использования
    public OrderRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Конструктор для Unit of Work с транзакцией
    public OrderRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        return _connection ?? await _connectionFactory.CreateConnectionAsync();
    }

    public async Task<OrderAggregate> GetByIdAsync(Guid orderId)
    {
        var connection = await GetConnectionAsync();

        var order = await connection.QueryFirstOrDefaultAsync<OrderAggregate>(
            "SELECT * FROM orders WHERE id = @OrderId",
            new { OrderId = orderId },
            _transaction);

        if (order == null) return null;

        var lines = await connection.QueryAsync<OrderLine>(
            "SELECT * FROM order_lines WHERE order_id = @OrderId",
            new { OrderId = orderId },
            _transaction);

        order.SetOrderLines(lines.AsList());

        return order;
    }

    public async Task<List<OrderAggregate>> GetOrdersByStatusAsync(OrderStatus status)
    {
        var connection = await GetConnectionAsync();

        var orders = await connection.QueryAsync<OrderAggregate>(
            "SELECT * FROM orders WHERE status = @Status ORDER BY created_at",
            new { Status = status.ToString() },
            _transaction);

        var result = new List<OrderAggregate>();
        foreach (var order in orders)
        {
            var lines = await connection.QueryAsync<OrderLine>(
                "SELECT * FROM order_lines WHERE order_id = @OrderId",
                new { OrderId = order.Id },
                _transaction);

            order.SetOrderLines(lines.AsList());
            result.Add(order);
        }

        return result;
    }

    public async Task<bool> ExistsAsync(Guid orderId)
    {
        var connection = await GetConnectionAsync();
        return await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM orders WHERE id = @OrderId)",
            new { OrderId = orderId },
            _transaction);
    }

    public async Task<List<OrderAggregate>> GetOrdersByPickerAsync(string pickerId)
    {
        var connection = await GetConnectionAsync();

        var orders = await connection.QueryAsync<OrderAggregate>(@"
            SELECT DISTINCT o.* FROM orders o
            INNER JOIN picking_tasks pt ON o.id = pt.order_id
            WHERE pt.assigned_picker = @PickerId",
            new { PickerId = pickerId },
            _transaction);

        var result = new List<OrderAggregate>();
        foreach (var order in orders)
        {
            var lines = await connection.QueryAsync<OrderLine>(
                "SELECT * FROM order_lines WHERE order_id = @OrderId",
                new { OrderId = order.Id },
                _transaction);

            order.SetOrderLines(lines.AsList());
            result.Add(order);
        }

        return result;
    }

    public async Task<List<OrderAggregate>> GetOrdersCreatedAfterAsync(DateTime date)
    {
        var connection = await GetConnectionAsync();

        var orders = await connection.QueryAsync<OrderAggregate>(
            "SELECT * FROM orders WHERE created_at >= @Date ORDER BY created_at DESC",
            new { Date = date },
            _transaction);

        var result = new List<OrderAggregate>();
        foreach (var order in orders)
        {
            var lines = await connection.QueryAsync<OrderLine>(
                "SELECT * FROM order_lines WHERE order_id = @OrderId",
                new { OrderId = order.Id },
                _transaction);

            order.SetOrderLines(lines.AsList());
            result.Add(order);
        }

        return result;
    }

    // IRepository<T> методы
    public async Task AddAsync(OrderAggregate entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO orders (id, customer_id, status, created_at, picking_started_at, picking_completed_at)
            VALUES (@Id, @CustomerId, @Status, @CreatedAt, @PickingStartedAt, @PickingCompletedAt)",
            entity, _transaction);

        foreach (var line in entity.Lines)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO order_lines (order_id, product_id, product_name, sku, quantity_ordered, quantity_picked, unit_price)
                VALUES (@OrderId, @ProductId, @ProductName, @Sku, @QuantityOrdered, @QuantityPicked, @UnitPrice)",
                line, _transaction);
        }
    }

    public async Task UpdateAsync(OrderAggregate entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            UPDATE orders 
            SET customer_id = @CustomerId, status = @Status, 
                picking_started_at = @PickingStartedAt, picking_completed_at = @PickingCompletedAt
            WHERE id = @Id", entity, _transaction);

        // TODO: Более сложная логика для обновления линий заказа
    }

    public async Task DeleteAsync(OrderAggregate entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM order_lines WHERE order_id = @OrderId",
            new { OrderId = entity.Id },
            _transaction);

        await connection.ExecuteAsync(
            "DELETE FROM orders WHERE id = @Id",
            new { entity.Id },
            _transaction);
    }

    public async Task<IReadOnlyList<OrderAggregate>> GetAllAsync()
    {
        var connection = await GetConnectionAsync();

        var orders = await connection.QueryAsync<OrderAggregate>(
            "SELECT * FROM orders",
            transaction: _transaction);

        var result = new List<OrderAggregate>();

        foreach (var order in orders)
        {
            var lines = await connection.QueryAsync<OrderLine>(
                "SELECT * FROM order_lines WHERE order_id = @OrderId",
                new { OrderId = order.Id },
                _transaction);

            order.SetOrderLines(lines.AsList());
            result.Add(order);
        }

        return result;
    }
}