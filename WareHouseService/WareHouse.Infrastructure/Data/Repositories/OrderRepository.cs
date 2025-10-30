using System.Linq.Expressions;
using Dapper;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;
using WareHouse.Domain.Enums;

namespace WareHouse.Infrastructure.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly NpgsqlConnection? _connection;
    private readonly NpgsqlTransaction? _transaction;

    public OrderRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public OrderRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        return _connection ?? await _connectionFactory.CreateConnectionAsync();
    }

    public async Task<OrderAggregate> GetByIdAsync(Guid id)
    {
        var connection = await GetConnectionAsync();

        var orderResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM orders WHERE id = @Id",
            new { Id = id },
            _transaction);

        if (orderResult == null)
            throw new KeyNotFoundException($"Order with id {id} not found");

        var order = CreateOrderAggregateFromDynamic(orderResult);
        var lines = await GetOrderLinesAsync(id);

        order.SetOrderLines(lines);

        return order;
    }

    public async Task<List<OrderAggregate>> GetByStatusAsync(OrderStatus status)
    {
        var connection = await GetConnectionAsync();

        var orderResults = await connection.QueryAsync<dynamic>(
            "SELECT * FROM orders WHERE status = @Status ORDER BY created_at DESC",
            new { Status = status.ToString() },
            _transaction);

        var orders = new List<OrderAggregate>();

        foreach (var orderResult in orderResults)
        {
            var order = CreateOrderAggregateFromDynamic(orderResult);
            var lines = await GetOrderLinesAsync(order.OrderId);

            order.SetOrderLines(lines);
            orders.Add(order);
        }

        return orders;
    }

    public async Task<List<OrderLine>> GetOrderLinesAsync(Guid orderId)
    {
        var connection = await GetConnectionAsync();

        var results = await connection.QueryAsync<dynamic>(
            "SELECT * FROM order_lines WHERE order_id = @OrderId",
            new { OrderId = orderId },
            _transaction);

        var lines = new List<OrderLine>();

        foreach (var result in results)
        {
            var line = CreateOrderLineFromDynamic(result);
            lines.Add(line);
        }

        return lines;
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            UPDATE orders SET status = @Status WHERE id = @OrderId",
            new { OrderId = orderId, Status = status.ToString() },
            _transaction);
    }

    // IRepository<T> методы
    public async Task<OrderAggregate> AddAsync(OrderAggregate entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO orders (id, customer_id, status, created_at, picking_started_at, picking_completed_at)
            VALUES (@Id, @CustomerId, @Status, @CreatedAt, @PickingStartedAt, @PickingCompletedAt)",
            new
            {
                Id = entity.OrderId,
                CustomerId = entity.CustomerId,
                Status = entity.Status.ToString(),
                CreatedAt = entity.CreatedAt,
                PickingStartedAt = entity.PickingStartedAt,
                PickingCompletedAt = entity.PickingCompletedAt

            },
            _transaction);

        foreach (var line in entity.Lines)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO order_lines (order_id, product_id, product_name, sku, quantity_ordered, quantity_picked, unit_price)
                VALUES (@OrderId, @ProductId, @ProductName, @Sku, @QuantityOrdered, @QuantityPicked, @UnitPrice)",
                new
                {
                    OrderId = line.OrderId,
                    ProductId = line.ProductId,
                    ProductName = line.ProductName,
                    Sku = line.Sku,
                    QuantityOrdered = line.QuantityOrdered,
                    QuantityPicked = line.QuantityPicked,
                    UnitPrice = line.UnitPrice
                },
                _transaction);
        }

        return entity;
    }

    public async Task UpdateAsync(OrderAggregate entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            UPDATE orders 
            SET customer_id = @CustomerId, status = @Status, 
                picking_started_at = @PickingStartedAt, 
                picking_completed_at = @PickingCompletedAt
            WHERE id = @Id",
            new
            {
                Id = entity.OrderId,
                CustomerId = entity.CustomerId,
                Status = entity.Status.ToString(),
                PickingStartedAt = entity.PickingStartedAt,
                PickingCompletedAt = entity.PickingCompletedAt,
            },
            _transaction);

        await connection.ExecuteAsync(
            "DELETE FROM order_lines WHERE order_id = @OrderId",
            new { OrderId = entity.OrderId },
            _transaction);

        foreach (var line in entity.Lines)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO order_lines (order_id, product_id, product_name, sku, quantity_ordered, quantity_picked, unit_price)
                VALUES (@OrderId, @ProductId, @ProductName, @Sku, @QuantityOrdered, @QuantityPicked, @UnitPrice)",
                new
                {
                    OrderId = line.OrderId,
                    ProductId = line.ProductId,
                    ProductName = line.ProductName,
                    Sku = line.Sku,
                    QuantityOrdered = line.QuantityOrdered,
                    QuantityPicked = line.QuantityPicked,
                    UnitPrice = line.UnitPrice
                },
                _transaction);
        }
    }

    public async Task DeleteAsync(OrderAggregate entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM order_lines WHERE order_id = @OrderId",
            new { OrderId = entity.OrderId },
            _transaction);

        await connection.ExecuteAsync(
            "DELETE FROM orders WHERE id = @Id",
            new { Id = entity.OrderId },
            _transaction);
    }

    public async Task<IReadOnlyList<OrderAggregate>> GetAllAsync()
    {
        var connection = await GetConnectionAsync();

        var orderResults = await connection.QueryAsync<dynamic>(
            "SELECT * FROM orders ORDER BY created_at DESC",
            transaction: _transaction);

        var orders = new List<OrderAggregate>();

        foreach (var orderResult in orderResults)
        {
            var order = CreateOrderAggregateFromDynamic(orderResult);
            var lines = await GetOrderLinesAsync(order.OrderId);

            order.SetOrderLines(lines);
            orders.Add(order);
        }

        return orders;
    }

    public async Task<IReadOnlyList<OrderAggregate>> GetAsync(Expression<Func<OrderAggregate, bool>> predicate)
    {
        var allOrders = await GetAllAsync();
        return allOrders.Where(predicate.Compile()).ToList();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        var connection = await GetConnectionAsync();

        var exists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM orders WHERE id = @Id)",
            new { Id = id },
            _transaction);

        return exists;
    }

    public async Task<int> CountAsync()
    {
        var connection = await GetConnectionAsync();

        var count = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM orders",
            transaction: _transaction);

        return count;
    }

    public async Task SaveChangesAsync()
    {
        // Для Dapper обычно не требуется отдельный SaveChanges
        // Но если используется транзакция, можно закоммитить её здесь
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
        }
    }

    private OrderAggregate CreateOrderAggregateFromDynamic(dynamic result)
    {
        // Используем reflection для вызова приватного конструктора
        var orderType = typeof(OrderAggregate);
        var order = (OrderAggregate)Activator.CreateInstance(orderType, true)!;

        // Устанавливаем свойства через reflection
        SetProperty(order, "OrderId", result.id);
        SetProperty(order, "CustomerId", result.customer_id);
        SetProperty(order, "CreatedAt", result.created_at);
        SetProperty(order, "PickingStartedAt", result.picking_started_at);
        SetProperty(order, "PickingCompletedAt", result.picking_completed_at);

        // Устанавливаем статус
        var status = Enum.Parse<OrderStatus>(result.status);
        SetProperty(order, "Status", status);

        return order;
    }

    private OrderLine CreateOrderLineFromDynamic(dynamic result)
    {
        // Создаем OrderLine через reflection (предполагая, что у него тоже есть приватный конструктор)
        var lineType = typeof(OrderLine);
        var line = (OrderLine)Activator.CreateInstance(lineType, true)!;

        SetProperty(line, "Id", result.id ?? Guid.NewGuid());
        SetProperty(line, "OrderId", result.order_id);
        SetProperty(line, "ProductId", result.product_id);
        SetProperty(line, "ProductName", result.product_name);
        SetProperty(line, "Sku", result.sku);
        SetProperty(line, "QuantityOrdered", result.quantity_ordered);
        SetProperty(line, "QuantityPicked", result.quantity_picked);
        SetProperty(line, "UnitPrice", result.unit_price);

        return line;
    }

    private void SetProperty(object obj, string propertyName, object value)
    {
        if (value == null) return;

        try
        {
            var property = obj.GetType().GetProperty(propertyName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            if (property != null && property.CanWrite)
            {
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                // Специальная обработка для enum
                if (property.PropertyType.IsEnum && value is string stringValue)
                {
                    var enumValue = Enum.Parse(property.PropertyType, stringValue);
                    property.SetValue(obj, enumValue);
                }
                // Специальная обработка для Guid
                else if (targetType == typeof(Guid) && value is string guidString)
                {
                    var guidValue = Guid.Parse(guidString);
                    property.SetValue(obj, guidValue);
                }
                else if (value.GetType() != targetType)
                {
                    var convertedValue = Convert.ChangeType(value, targetType);
                    property.SetValue(obj, convertedValue);
                }
                else
                {
                    property.SetValue(obj, value);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting property {propertyName}: {ex.Message}");
            throw;
        }
    }
}