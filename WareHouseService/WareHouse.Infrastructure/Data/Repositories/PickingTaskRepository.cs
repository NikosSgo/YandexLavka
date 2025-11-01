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

        var taskResult = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM picking_tasks WHERE id = @TaskId",
            new { TaskId = taskId },
            _transaction);

        if (taskResult == null) return null;

        var itemsResults = await connection.QueryAsync<dynamic>(@"
        SELECT 
            pi.*,
            p.name as product_name,
            p.sku as sku
        FROM picking_items pi
        LEFT JOIN products p ON pi.product_id = p.id
        WHERE pi.picking_task_id = @TaskId",
            new { TaskId = taskId },
            _transaction);

        var items = itemsResults.Select(MapToPickingItem).Where(x => x != null).ToList();

        var task = MapToPickingTask(taskResult);
        task.SetPickingItems(items);

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

        var parameters = new
        {
            Id = entity.TaskId,
            OrderId = entity.OrderId,
            AssignedPicker = entity.AssignedPicker,
            Status = EnumConverter.ToString(entity.Status),
            Zone = entity.Zone,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt
        };

        await connection.ExecuteAsync(@"
        INSERT INTO picking_tasks 
            (id, order_id, assigned_picker, status, zone, created_at, completed_at)
        VALUES 
            (@Id, @OrderId, @AssignedPicker, @Status, @Zone, @CreatedAt, @CompletedAt)",
            parameters, _transaction);

        foreach (var item in entity.Items)
        {
            var itemParameters = new
            {
                PickingTaskId = entity.TaskId,
                ProductId = item.ProductId,
                // ❌ УБИРАЕМ: ProductName = item.ProductName,
                // ❌ УБИРАЕМ: Sku = item.Sku,
                Quantity = item.Quantity,
                StorageLocation = item.StorageLocation,
                Barcode = item.Barcode
            };

            await connection.ExecuteAsync(@"
            INSERT INTO picking_items 
                (picking_task_id, product_id, quantity, storage_location, barcode)
            VALUES 
                (@PickingTaskId, @ProductId, @Quantity, @StorageLocation, @Barcode)",
                itemParameters, _transaction);
        }
    }
    public async Task UpdateAsync(PickingTask entity)
    {
        var connection = await GetConnectionAsync();

        var parameters = new
        {
            Id = entity.TaskId,
            AssignedPicker = entity.AssignedPicker,
            Status = EnumConverter.ToString(entity.Status),
            Zone = entity.Zone,
            CompletedAt = entity.CompletedAt
            // ❌ УБИРАЕМ: StartedAt = entity.StartedAt,
        };

        await connection.ExecuteAsync(@"
        UPDATE picking_tasks 
        SET 
            assigned_picker = @AssignedPicker,
            status = @Status,
            zone = @Zone,
            completed_at = @CompletedAt
        WHERE id = @Id",
            parameters, _transaction);
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

    // ✅ ДОБАВЛЯЕМ: Метод маппинга для PickingTask
    private PickingTask MapToPickingTask(dynamic result)
    {
        if (result == null) return null;

        try
        {
            var task = new PickingTask();

            // Используем reflection для установки свойств
            SetProperty(task, "TaskId", result.id);
            SetProperty(task, "OrderId", result.order_id);
            SetProperty(task, "AssignedPicker", result.assigned_picker);

            // Конвертируем строковый статус в enum
            var status = EnumConverter.ToPickingTaskStatus(result.status);
            SetProperty(task, "Status", status);

            SetProperty(task, "CreatedAt", result.created_at);
            SetProperty(task, "CompletedAt", result.completed_at);
            SetProperty(task, "Zone", result.zone);

            return task;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error mapping PickingTask: {ex.Message}");
            return null;
        }
    }

    // ✅ ДОБАВЛЯЕМ: Метод маппинга для PickingItem
    private PickingItem MapToPickingItem(dynamic result)
    {
        if (result == null) return null;

        try
        {
            return new PickingItem(
                pickingTaskId: result.picking_task_id,
                productId: result.product_id,
                productName: result.product_name ?? "Unknown Product",
                sku: result.sku ?? "Unknown SKU",
                quantity: result.quantity,
                storageLocation: result.storage_location,
                barcode: result.barcode
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error mapping PickingItem: {ex.Message}");
            return null;
        }
    }

    // ✅ ДОБАВЛЯЕМ: Метод для установки свойств через reflection
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

    // ✅ ДОБАВЛЯЕМ: Отсутствующий метод AddAsync с возвращаемым значением
    //public async Task<PickingTask> AddAsync(PickingTask entity)
    //{
    //    var connection = await GetConnectionAsync();

    //    var parameters = new
    //    {
    //        Id = entity.TaskId,
    //        OrderId = entity.OrderId,
    //        AssignedPicker = entity.AssignedPicker,
    //        Status = EnumConverter.ToString(entity.Status),
    //        Zone = entity.Zone,
    //        CreatedAt = entity.CreatedAt,
    //        CompletedAt = entity.CompletedAt
    //    };

    //    await connection.ExecuteAsync(@"
    //    INSERT INTO picking_tasks 
    //        (id, order_id, assigned_picker, status, zone, created_at, completed_at)
    //    VALUES 
    //        (@Id, @OrderId, @AssignedPicker, @Status, @Zone, @CreatedAt, @CompletedAt)",
    //        parameters, _transaction);

    //    foreach (var item in entity.Items)
    //    {
    //        var itemParameters = new
    //        {
    //            PickingTaskId = entity.TaskId,
    //            ProductId = item.ProductId,
    //            Quantity = item.Quantity,
    //            StorageLocation = item.StorageLocation,
    //            Barcode = item.Barcode
    //        };

    //        await connection.ExecuteAsync(@"
    //        INSERT INTO picking_items 
    //            (picking_task_id, product_id, quantity, storage_location, barcode)
    //        VALUES 
    //            (@PickingTaskId, @ProductId, @Quantity, @StorageLocation, @Barcode)",
    //            itemParameters, _transaction);
    //    }

    //    return entity; // ✅ Возвращаем добавленную сущность
    //}
}