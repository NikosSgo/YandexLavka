using Dapper;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class PickingTaskRepository : IPickingTaskRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    // Конструктор для обычного использования
    public PickingTaskRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Конструктор для Unit of Work с транзакцией
    public PickingTaskRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        return _connection ?? await _connectionFactory.CreateConnectionAsync();
    }

    public async Task<PickingTask> GetByIdAsync(Guid taskId)
    {
        var connection = await GetConnectionAsync();

        var task = await connection.QueryFirstOrDefaultAsync<PickingTask>(
            "SELECT * FROM picking_tasks WHERE id = @TaskId",
            new { TaskId = taskId },
            _transaction);

        if (task == null) return null;

        var items = await connection.QueryAsync<PickingItem>(
            "SELECT * FROM picking_items WHERE picking_task_id = @TaskId",
            new { TaskId = taskId },
            _transaction);

        task.SetPickingItems(items.AsList());

        return task;
    }

    public async Task<PickingTask> GetActiveForOrderAsync(Guid orderId)
    {
        var connection = await GetConnectionAsync();

        var task = await connection.QueryFirstOrDefaultAsync<PickingTask>(@"
            SELECT * FROM picking_tasks 
            WHERE order_id = @OrderId AND status IN ('Created', 'InProgress')
            LIMIT 1",
            new { OrderId = orderId },
            _transaction);

        if (task == null) return null;

        var items = await connection.QueryAsync<PickingItem>(
            "SELECT * FROM picking_items WHERE picking_task_id = @TaskId",
            new { TaskId = task.Id },
            _transaction);

        task.SetPickingItems(items.AsList());

        return task;
    }

    public async Task<List<PickingTask>> GetByPickerAsync(string pickerId)
    {
        var connection = await GetConnectionAsync();

        var tasks = await connection.QueryAsync<PickingTask>(
            "SELECT * FROM picking_tasks WHERE assigned_picker = @PickerId ORDER BY created_at DESC",
            new { PickerId = pickerId },
            _transaction);

        var result = new List<PickingTask>();
        foreach (var task in tasks)
        {
            var items = await connection.QueryAsync<PickingItem>(
                "SELECT * FROM picking_items WHERE picking_task_id = @TaskId",
                new { TaskId = task.Id },
                _transaction);

            task.SetPickingItems(items.AsList());
            result.Add(task);
        }

        return result;
    }

    public async Task<List<PickingTask>> GetTasksByStatusAsync(PickingTaskStatus status)
    {
        var connection = await GetConnectionAsync();

        var tasks = await connection.QueryAsync<PickingTask>(
            "SELECT * FROM picking_tasks WHERE status = @Status ORDER BY created_at",
            new { Status = status.ToString() },
            _transaction);

        var result = new List<PickingTask>();
        foreach (var task in tasks)
        {
            var items = await connection.QueryAsync<PickingItem>(
                "SELECT * FROM picking_items WHERE picking_task_id = @TaskId",
                new { TaskId = task.Id },
                _transaction);

            task.SetPickingItems(items.AsList());
            result.Add(task);
        }

        return result;
    }

    public async Task<List<PickingTask>> GetTasksByZoneAsync(string zone)
    {
        var connection = await GetConnectionAsync();

        var tasks = await connection.QueryAsync<PickingTask>(@"
            SELECT * FROM picking_tasks 
            WHERE zone = @Zone AND status IN ('Created', 'InProgress')
            ORDER BY created_at",
            new { Zone = zone },
            _transaction);

        var result = new List<PickingTask>();
        foreach (var task in tasks)
        {
            var items = await connection.QueryAsync<PickingItem>(
                "SELECT * FROM picking_items WHERE picking_task_id = @TaskId",
                new { TaskId = task.Id },
                _transaction);

            task.SetPickingItems(items.AsList());
            result.Add(task);
        }

        return result;
    }

    // IRepository<T> методы
    public async Task AddAsync(PickingTask entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO picking_tasks (id, order_id, assigned_picker, status, zone, created_at, completed_at)
            VALUES (@Id, @OrderId, @AssignedPicker, @Status, @Zone, @CreatedAt, @CompletedAt)",
            entity, _transaction);

        foreach (var item in entity.Items)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO picking_items (picking_task_id, product_id, product_name, sku, quantity, storage_location, barcode)
                VALUES (@PickingTaskId, @ProductId, @ProductName, @Sku, @Quantity, @StorageLocation, @Barcode)",
                item, _transaction);
        }
    }

    public async Task UpdateAsync(PickingTask entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(@"
            UPDATE picking_tasks 
            SET order_id = @OrderId, assigned_picker = @AssignedPicker, status = @Status,
                zone = @Zone, completed_at = @CompletedAt
            WHERE id = @Id",
            entity, _transaction);

        // TODO: Логика обновления picking items
    }

    public async Task DeleteAsync(PickingTask entity)
    {
        var connection = await GetConnectionAsync();

        await connection.ExecuteAsync(
            "DELETE FROM picking_items WHERE picking_task_id = @TaskId",
            new { TaskId = entity.Id },
            _transaction);

        await connection.ExecuteAsync(
            "DELETE FROM picking_tasks WHERE id = @Id",
            new { entity.Id },
            _transaction);
    }

    public async Task<IReadOnlyList<PickingTask>> GetAllAsync()
    {
        var connection = await GetConnectionAsync();

        var tasks = await connection.QueryAsync<PickingTask>(
            "SELECT * FROM picking_tasks",
            transaction: _transaction);

        var result = new List<PickingTask>();

        foreach (var task in tasks)
        {
            var items = await connection.QueryAsync<PickingItem>(
                "SELECT * FROM picking_items WHERE picking_task_id = @TaskId",
                new { TaskId = task.Id },
                _transaction);

            task.SetPickingItems(items.AsList());
            result.Add(task);
        }

        return result;
    }
}