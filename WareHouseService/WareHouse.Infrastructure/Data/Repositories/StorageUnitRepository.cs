// WareHouse.Infrastructure/Data/Repositories/StorageUnitRepository.cs
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

    public async Task<StorageUnit> GetByIdAsync(Guid id)
    {
        var connection = await GetConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<StorageUnit>(
            "SELECT * FROM storage_units WHERE id = @Id",
            new { Id = id },
            _transaction);
    }

    public async Task<StorageUnit> GetByProductAndLocationAsync(Guid productId, string location)
    {
        var connection = await GetConnectionAsync();
        return await connection.QueryFirstOrDefaultAsync<StorageUnit>(
            "SELECT * FROM storage_units WHERE product_id = @ProductId AND location = @Location",
            new { ProductId = productId, Location = location },
            _transaction);
    }

    public async Task<List<StorageUnit>> GetByProductAsync(Guid productId)
    {
        var connection = await GetConnectionAsync();
        var result = await connection.QueryAsync<StorageUnit>(
            "SELECT * FROM storage_units WHERE product_id = @ProductId ORDER BY location",
            new { ProductId = productId },
            _transaction);
        return result.AsList();
    }

    public async Task<List<StorageUnit>> GetByLocationAsync(string location)
    {
        var connection = await GetConnectionAsync();
        var result = await connection.QueryAsync<StorageUnit>(
            "SELECT * FROM storage_units WHERE location = @Location",
            new { Location = location },
            _transaction);
        return result.AsList();
    }

    public async Task<List<StorageUnit>> GetByZoneAsync(string zone)
    {
        var connection = await GetConnectionAsync();
        var result = await connection.QueryAsync<StorageUnit>(
            "SELECT * FROM storage_units WHERE zone = @Zone ORDER BY location",
            new { Zone = zone },
            _transaction);
        return result.AsList();
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
        var result = await connection.QueryAsync<StorageUnit>(@"
            SELECT * FROM storage_units 
            WHERE product_id = ANY(@ProductIds) AND (quantity - reserved_quantity) > 0
            ORDER BY zone, location",
            new { ProductIds = productIds.ToArray() },
            _transaction);

        return result.AsList();
    }

    public async Task<List<StorageUnit>> GetLowStockUnitsAsync()
    {
        var connection = await GetConnectionAsync();
        var result = await connection.QueryAsync<StorageUnit>(@"
            SELECT * FROM storage_units 
            WHERE (quantity - reserved_quantity) <= 10 
            ORDER BY (quantity - reserved_quantity)",
            new { },
            _transaction);
        return result.AsList();
    }

    // IRepository<T> методы
    public async Task AddAsync(StorageUnit entity)
    {
        var connection = await GetConnectionAsync();
        await connection.ExecuteAsync(@"
            INSERT INTO storage_units (id, product_id, product_name, sku, quantity, reserved_quantity, 
                                     location, zone, last_restocked, ""CreatedAt"", ""UpdatedAt"", 
                                     ""IsLowStock"", ""IsOutOfStock"")
            VALUES (@Id, @ProductId, @ProductName, @Sku, @Quantity, @ReservedQuantity, 
                   @Location, @Zone, @LastRestocked, @CreatedAt, @UpdatedAt, 
                   @IsLowStock, @IsOutOfStock)",
            entity, _transaction);
    }

    public async Task UpdateAsync(StorageUnit entity)
    {
        var connection = await GetConnectionAsync();
        await connection.ExecuteAsync(@"
            UPDATE storage_units 
            SET product_id = @ProductId, product_name = @ProductName, sku = @Sku,
                quantity = @Quantity, reserved_quantity = @ReservedQuantity,
                location = @Location, zone = @Zone, last_restocked = @LastRestocked,
                ""UpdatedAt"" = @UpdatedAt, ""IsLowStock"" = @IsLowStock, 
                ""IsOutOfStock"" = @IsOutOfStock
            WHERE id = @Id",
            entity, _transaction);
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
        var result = await connection.QueryAsync<StorageUnit>(
            "SELECT * FROM storage_units",
            transaction: _transaction);
        return result.AsList();
    }
}