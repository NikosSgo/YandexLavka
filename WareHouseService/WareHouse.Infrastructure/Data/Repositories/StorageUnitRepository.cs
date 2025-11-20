using Dapper;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;
using WareHouse.Domain.ValueObjects;

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

        var query = @"
        SELECT 
            su.*, 
            p.name as product_name, 
            p.sku, 
            p.category
        FROM storage_units su
        INNER JOIN products p ON su.product_id = p.id
        WHERE su.product_id IN (
            SELECT DISTINCT ol.product_id 
            FROM order_lines ol 
            WHERE ol.order_id = @OrderId
        )
        AND su.quantity > su.reserved_quantity
        ORDER BY 
            CASE su.zone
                WHEN 'A' THEN 1
                WHEN 'B' THEN 2  
                WHEN 'C' THEN 3
                WHEN 'cooler' THEN 4
                WHEN 'freezer' THEN 5
                ELSE 6
            END,
            su.location";

        // Логируем запрос и параметры
        System.Diagnostics.Debug.WriteLine($"GetUnitsForOrderAsync - OrderId: {orderId}");

        var results = await connection.QueryAsync<dynamic>(query, new { OrderId = orderId }, _transaction);

        // Логируем результаты
        foreach (var result in results)
        {
            System.Diagnostics.Debug.WriteLine($"Result - ProductId: {result.product_id}, ProductName: {result.product_name}, Sku: {result.sku}");
        }

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    public async Task<List<StorageUnit>> GetLowStockUnitsAsync()
    {
        try
        {
            var connection = await GetConnectionAsync();
            var results = await connection.QueryAsync<dynamic>(@"
            SELECT 
                su.*, 
                p.name as product_name, 
                p.sku, 
                p.category
            FROM storage_units su
            LEFT JOIN products p ON su.product_id = p.id
            WHERE (su.quantity - su.reserved_quantity) <= 10 
            ORDER BY (su.quantity - su.reserved_quantity)",
                new { },
                _transaction);

            var units = results.Select(MapToStorageUnit).Where(x => x != null).ToList();
            return units;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetLowStockUnitsAsync: {ex.Message}");
            throw;
        }
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
            // ❌ УБРАТЬ: ProductName = entity.ProductName,
            // ❌ УБРАТЬ: Sku = entity.Sku,
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

    public async Task<StorageUnit?> GetStockLevelByProductIdAsync(Guid productId)
    {
        var connection = await GetConnectionAsync();

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(@"
        SELECT 
            su.*, 
            p.name as product_name, 
            p.sku, 
            p.category
        FROM storage_units su
        LEFT JOIN products p ON su.product_id = p.id
        WHERE su.product_id = @ProductId
        LIMIT 1",  // Берем первую найденную единицу
            new { ProductId = productId },
            _transaction);

        if (result == null) return null;

        return MapToStorageUnit(result);
    }

    public async Task<List<StorageUnit>> GetStockLevelsByProductIdsAsync(List<Guid> productIds)
    {
        var connection = await GetConnectionAsync();

        var results = await connection.QueryAsync<dynamic>(@"
        SELECT 
            su.*, 
            p.name as product_name, 
            p.sku, 
            p.category
        FROM storage_units su
        LEFT JOIN products p ON su.product_id = p.id
        WHERE su.product_id = ANY(@ProductIds)",
            new { ProductIds = productIds.ToArray() },
            _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
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
        var results = await connection.QueryAsync<dynamic>(@"
            SELECT
                su.*,
                p.name as product_name,
                p.sku
            FROM storage_units su
            LEFT JOIN products p ON su.product_id = p.id",
            transaction: _transaction);

        return results.Select(MapToStorageUnit).Where(x => x != null).ToList();
    }

    public async Task<List<string>> GetZonesForOrderAsync(Guid orderId)
    {
        var connection = await GetConnectionAsync();

        try
        {
            var query = @"
        SELECT DISTINCT su.zone
        FROM storage_units su
        INNER JOIN order_lines ol ON su.product_id = ol.product_id
        WHERE ol.order_id = @OrderId
        AND (su.quantity - su.reserved_quantity) > 0
        AND su.zone IS NOT NULL
        AND su.zone != ''"; // ✅ Теперь zone есть в SELECT

            var zones = await connection.QueryAsync<string>(query, new { OrderId = orderId }, _transaction);

            // ✅ Возвращаем оригинальные названия зон
            var result = zones?
                .Where(zone => !string.IsNullOrEmpty(zone))
                .Distinct()
                .ToList() ?? new List<string>();
            
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message} getting zones for order {orderId}");
            throw;
        }
    }

    private StorageUnit MapToStorageUnit(dynamic result)
    {
        if (result == null) return null;

        try
        {
            var storageUnit = new StorageUnit();

            // Логируем что приходит из БД
            System.Diagnostics.Debug.WriteLine($"Mapping StorageUnit - ProductId: {result.product_id}, ProductName from DB: {result.product_name}");

            // Устанавливаем свойства
            SetProperty(storageUnit, "Id", result.id);
            SetProperty(storageUnit, "ProductId", result.product_id);

            // Проверяем есть ли product_name из JOIN
            string productName = result.product_name;
            string sku = result.sku;

            if (string.IsNullOrEmpty(productName))
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: No product_name found for product_id: {result.product_id}");
                productName = $"Product-{result.product_id}";
            }

            if (string.IsNullOrEmpty(sku))
            {
                System.Diagnostics.Debug.WriteLine($"WARNING: No sku found for product_id: {result.product_id}");
                sku = $"SKU-{result.product_id}";
            }

            SetProperty(storageUnit, "ProductName", productName);
            SetProperty(storageUnit, "Sku", sku);
            SetProperty(storageUnit, "Quantity", result.quantity);
            SetProperty(storageUnit, "ReservedQuantity", result.reserved_quantity);
            SetProperty(storageUnit, "Location", result.location ?? "UNKNOWN");
            SetProperty(storageUnit, "Zone", result.zone ?? "DEFAULT");
            SetProperty(storageUnit, "LastRestocked", result.last_restocked);
            SetProperty(storageUnit, "CreatedAt", result.CreatedAt ?? DateTime.UtcNow);
            SetProperty(storageUnit, "UpdatedAt", result.UpdatedAt ?? DateTime.UtcNow);

            storageUnit.UpdateStockFlags();

            return storageUnit;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error mapping StorageUnit: {ex.Message}");
            return null;
        }
    }

    //private void SetProperty(object obj, string propertyName, object value)
    //{
    //    if (value == null) return;

    //    try
    //    {
    //        var property = obj.GetType().GetProperty(propertyName,
    //            System.Reflection.BindingFlags.Public |
    //            System.Reflection.BindingFlags.NonPublic |
    //            System.Reflection.BindingFlags.Instance);

    //        if (property != null && property.CanWrite)
    //        {
    //            // Конвертируем значение к правильному типу
    //            var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
    //            var convertedValue = Convert.ChangeType(value, targetType);
    //            property.SetValue(obj, convertedValue);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        System.Diagnostics.Debug.WriteLine($"Error setting property {propertyName}: {ex.Message}");
    //    }
    //}



    private bool HasProperty(dynamic obj, string propertyName)
    {
        try
        {
            var value = ((object)obj).GetType().GetProperty(propertyName);
            return value != null;
        }
        catch
        {
            return false;
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
                var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                if (value.GetType() != targetType)
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