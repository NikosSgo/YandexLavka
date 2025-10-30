using Dapper;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class StorageUnitRepository : IStorageUnitRepository
{
    private readonly IDatabaseConnectionFactory _connectionFactory;
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlTransaction _transaction;

    public StorageUnitRepository(IDatabaseConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public StorageUnitRepository(NpgsqlConnection connection, NpgsqlTransaction transaction)
    {
        _connection = connection;
        _transaction = transaction;
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        return _connection ?? await _connectionFactory.CreateConnectionAsync();
    }

    public async Task<StorageUnit?> GetByIdAsync(Guid id)
    {
        var connection = await GetConnectionAsync();

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM storage_units WHERE id = @Id",
            new { Id = id },
            _transaction);

        if (result == null) return null;

        return MapToStorageUnit(result);
    }

    public async Task<StorageUnit?> GetByProductAndLocationAsync(Guid productId, string location)
    {
        var connection = await GetConnectionAsync();

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT * FROM storage_units WHERE product_id = @ProductId AND location = @Location",
            new { ProductId = productId, Location = location },
            _transaction);

        if (result == null) return null;

        return MapToStorageUnit(result);
    }

    public async Task<List<StorageUnit>> GetByProductAsync(Guid productId)
    {
        var connection = await GetConnectionAsync();
        var results = await connection.QueryAsync<dynamic>(
            "SELECT * FROM storage_units WHERE product_id = @ProductId ORDER BY location",
            new { ProductId = productId },
            _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    public async Task<List<StorageUnit>> GetByLocationAsync(string location)
    {
        var connection = await GetConnectionAsync();
        var results = await connection.QueryAsync<dynamic>(
            "SELECT * FROM storage_units WHERE location = @Location",
            new { Location = location },
            _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    public async Task<List<StorageUnit>> GetByZoneAsync(string zone)
    {
        var connection = await GetConnectionAsync();
        var results = await connection.QueryAsync<dynamic>(
            "SELECT * FROM storage_units WHERE zone = @Zone ORDER BY location",
            new { Zone = zone },
            _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    public async Task<List<StorageUnit>> GetUnitsForOrderAsync(Guid orderId)
    {
        var connection = await GetConnectionAsync();

        // Сначала получаем продукты из заказа
        var productIds = await connection.QueryAsync<Guid>(
            "SELECT product_id FROM order_lines WHERE order_id = @OrderId",
            new { OrderId = orderId },
            _transaction);

        if (!productIds.Any()) return new List<StorageUnit>();

        // Затем получаем storage units для этих продуктов
        var results = await connection.QueryAsync<dynamic>(@"
            SELECT * FROM storage_units 
            WHERE product_id = ANY(@ProductIds) AND (quantity - reserved_quantity) > 0
            ORDER BY zone, location",
            new { ProductIds = productIds.ToArray() },
            _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    public async Task<List<StorageUnit>> GetLowStockUnitsAsync()
    {
        var connection = await GetConnectionAsync();
        var results = await connection.QueryAsync<dynamic>(@"
            SELECT * FROM storage_units 
            WHERE (quantity - reserved_quantity) <= 10 
            ORDER BY (quantity - reserved_quantity)",
            new { },
            _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    // IRepository<T> методы
    public async Task AddAsync(StorageUnit entity)
    {
        var connection = await GetConnectionAsync();

        var parameters = new
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,
            Sku = entity.Sku,
            Quantity = entity.Quantity,
            ReservedQuantity = entity.ReservedQuantity,
            Location = entity.Location,
            Zone = entity.Zone,
            LastRestocked = entity.LastRestocked,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsLowStock = entity.IsLowStock,
            IsOutOfStock = entity.IsOutOfStock
        };

        await connection.ExecuteAsync(@"
            INSERT INTO storage_units 
                (id, product_id, product_name, sku, quantity, reserved_quantity, 
                 location, zone, last_restocked, ""CreatedAt"", ""UpdatedAt"", 
                 ""IsLowStock"", ""IsOutOfStock"")
            VALUES 
                (@Id, @ProductId, @ProductName, @Sku, @Quantity, @ReservedQuantity, 
                 @Location, @Zone, @LastRestocked, @CreatedAt, @UpdatedAt, 
                 @IsLowStock, @IsOutOfStock)",
            parameters, _transaction);
    }

    public async Task UpdateAsync(StorageUnit entity)
    {
        var connection = await GetConnectionAsync();

        const string sql = @"
            UPDATE storage_units 
            SET 
                product_id = @ProductId, 
                product_name = @ProductName, 
                sku = @Sku,
                quantity = @Quantity, 
                reserved_quantity = @ReservedQuantity,
                location = @Location, 
                zone = @Zone, 
                last_restocked = @LastRestocked,
                ""UpdatedAt"" = @UpdatedAt, 
                ""IsLowStock"" = @IsLowStock, 
                ""IsOutOfStock"" = @IsOutOfStock
            WHERE id = @Id";

        var parameters = new
        {
            Id = entity.Id,
            ProductId = entity.ProductId,
            ProductName = entity.ProductName,
            Sku = entity.Sku,
            Quantity = entity.Quantity,
            ReservedQuantity = entity.ReservedQuantity,
            Location = entity.Location,
            Zone = entity.Zone,
            LastRestocked = entity.LastRestocked,
            UpdatedAt = DateTime.UtcNow,
            IsLowStock = entity.IsLowStock,
            IsOutOfStock = entity.IsOutOfStock
        };

        await connection.ExecuteAsync(sql, parameters, _transaction);
    }

    public async Task DeleteAsync(StorageUnit entity)
    {
        var connection = await GetConnectionAsync();
        await connection.ExecuteAsync(
            "DELETE FROM storage_units WHERE id = @Id",
            new { entity.Id },
            _transaction);
    }

    public async Task<IReadOnlyList<StorageUnit>> GetAllAsync()
    {
        var connection = await GetConnectionAsync();
        var results = await connection.QueryAsync<dynamic>(
            "SELECT * FROM storage_units",
            transaction: _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    private StorageUnit MapToStorageUnit(dynamic result)
    {
        if (result == null) return null;

        try
        {
            // Создаем StorageUnit через конструктор
            var storageUnit = new StorageUnit(
                productId: result.product_id,
                productName: result.product_name ?? $"Product-{result.product_id}",
                sku: result.sku ?? $"SKU-{result.product_id}",
                quantity: result.quantity,
                location: result.location ?? "UNKNOWN",
                zone: result.zone ?? "DEFAULT"
            );

            // Устанавливаем свойства через reflection
            SetProperty(storageUnit, "Id", result.id);
            SetProperty(storageUnit, "ReservedQuantity", result.reserved_quantity);
            SetProperty(storageUnit, "LastRestocked", result.last_restocked);
            SetProperty(storageUnit, "CreatedAt", result.CreatedAt);
            SetProperty(storageUnit, "UpdatedAt", result.UpdatedAt);

            // Устанавливаем флаги запаса через публичный метод
            storageUnit.UpdateStockFlags();

            return storageUnit;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error mapping StorageUnit: {ex.Message}");
            return null;
        }
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
                // Конвертируем значение к правильному типу
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var convertedValue = Convert.ChangeType(value, targetType);
                property.SetValue(obj, convertedValue);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error setting property {propertyName}: {ex.Message}");
        }
    }
}