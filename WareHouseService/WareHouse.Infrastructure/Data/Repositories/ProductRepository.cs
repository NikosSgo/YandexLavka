using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDatabaseConnectionFactory? _connectionFactory;
    private readonly NpgsqlConnection? _connection;
    private readonly NpgsqlTransaction? _transaction;
    private readonly ILogger _logger; // Используем не-generic ILogger

    // Конструктор для обычного использования (через Dependency Injection)
    public ProductRepository(IDatabaseConnectionFactory connectionFactory, ILogger<ProductRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // Конструктор для использования в UnitOfWork (с транзакцией)
    public ProductRepository(NpgsqlConnection connection, NpgsqlTransaction transaction, ILogger logger)
    {
        _connection = connection;
        _transaction = transaction;
        _logger = logger;
    }

    private async Task<NpgsqlConnection> GetConnectionAsync()
    {
        if (_connection != null)
            return _connection;

        if (_connectionFactory != null)
            return await _connectionFactory.CreateConnectionAsync();

        throw new InvalidOperationException("No connection available");
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM products WHERE id = @Id",
                new { Id = id },
                _transaction);

            if (result == null)
            {
                _logger.LogWarning("Product with id {ProductId} not found", id);
                return null;
            }

            return MapToProduct(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by id {ProductId}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM products WHERE id = @Id)",
                new { Id = id },
                _transaction);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if product exists {ProductId}", id);
            throw;
        }
    }

    public async Task<List<Product>> GetByCategoryAsync(string category)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var results = await connection.QueryAsync<dynamic>(
                "SELECT * FROM products WHERE category = @Category ORDER BY name",
                new { Category = category },
                _transaction);

            var products = new List<Product>();
            foreach (var result in results)
            {
                var product = MapToProduct(result);
                if (product != null)
                    products.Add(product);
            }
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category {Category}", category);
            throw;
        }
    }

    public async Task<List<Product>> GetAllAsync()
    {
        try
        {
            var connection = await GetConnectionAsync();

            var results = await connection.QueryAsync<dynamic>(
                "SELECT * FROM products ORDER BY name",
                transaction: _transaction);

            var products = new List<Product>();
            foreach (var result in results)
            {
                var product = MapToProduct(result);
                if (product != null)
                    products.Add(product);
            }
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            throw;
        }
    }

    public async Task<Product> AddAsync(Product entity)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var parameters = new
            {
                Id = entity.Id,
                Name = entity.Name,
                Sku = entity.Sku,
                Description = entity.Description,
                Category = entity.Category,
                UnitPrice = entity.UnitPrice,
                WeightKg = entity.WeightKg,
                RequiresRefrigeration = entity.RequiresRefrigeration,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            await connection.ExecuteAsync(@"
                INSERT INTO products 
                    (id, name, sku, description, category, unit_price, weight_kg, requires_refrigeration, created_at, updated_at)
                VALUES 
                    (@Id, @Name, @Sku, @Description, @Category, @UnitPrice, @WeightKg, @RequiresRefrigeration, @CreatedAt, @UpdatedAt)",
                parameters, _transaction);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding product {ProductId}", entity.Id);
            throw;
        }
    }

    public async Task UpdateAsync(Product entity)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var parameters = new
            {
                Id = entity.Id,
                Name = entity.Name,
                Sku = entity.Sku,
                Description = entity.Description,
                Category = entity.Category,
                UnitPrice = entity.UnitPrice,
                WeightKg = entity.WeightKg,
                RequiresRefrigeration = entity.RequiresRefrigeration,
                UpdatedAt = DateTime.UtcNow
            };

            await connection.ExecuteAsync(@"
                UPDATE products 
                SET 
                    name = @Name,
                    sku = @Sku,
                    description = @Description,
                    category = @Category,
                    unit_price = @UnitPrice,
                    weight_kg = @WeightKg,
                    requires_refrigeration = @RequiresRefrigeration,
                    updated_at = @UpdatedAt
                WHERE id = @Id",
                parameters, _transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", entity.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Product entity)
    {
        try
        {
            var connection = await GetConnectionAsync();

            await connection.ExecuteAsync(
                "DELETE FROM products WHERE id = @Id",
                new { entity.Id },
                _transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", entity.Id);
            throw;
        }
    }

    private Product? MapToProduct(dynamic result)
    {
        if (result == null)
        {
            _logger.LogWarning("MapToProduct: result is null");
            return null;
        }

        try
        {
            var product = new Product();

            // Используем reflection для установки Id
            var idProperty = typeof(Product).GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(product, result.id);
            }
            else
            {
                _logger.LogWarning("Cannot set Id property for product");
            }

            product.Name = result.name ?? "Unknown Product";
            product.Sku = result.sku ?? "Unknown SKU";
            product.Description = result.description;
            product.Category = result.category ?? "Unknown Category";
            product.UnitPrice = result.unit_price;
            product.WeightKg = result.weight_kg;
            product.RequiresRefrigeration = result.requires_refrigeration;
            product.CreatedAt = result.created_at;
            product.UpdatedAt = result.updated_at;

            _logger.LogDebug("Mapped product: {ProductId}, Name: {ProductName}", product.Id, product.Name);

            return product;
        }
        catch (Exception ex)
        {
            // Используем статический вызов для избежания проблем с динамической диспетчеризацией
            LoggerExtensions.LogError(_logger, ex, "Error mapping Product from result");
            throw;
        }
    }
}