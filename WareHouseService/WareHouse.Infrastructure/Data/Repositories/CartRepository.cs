using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class CartRepository : ICartRepository
{
    private readonly IDatabaseConnectionFactory? _connectionFactory;
    private readonly NpgsqlConnection? _connection;
    private readonly NpgsqlTransaction? _transaction;
    private readonly ILogger _logger;

    // Конструктор для обычного использования (через Dependency Injection)
    public CartRepository(IDatabaseConnectionFactory connectionFactory, ILogger<CartRepository> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    // Конструктор для использования в UnitOfWork (с транзакцией)
    public CartRepository(NpgsqlConnection connection, NpgsqlTransaction transaction, ILogger logger)
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

    public async Task<CartItem?> GetByIdAsync(Guid id)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM cart_items WHERE id = @Id",
                new { Id = id },
                _transaction);

            if (result == null)
            {
                _logger.LogWarning("CartItem with id {CartItemId} not found", id);
                return null;
            }

            return MapToCartItem(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart item by id {CartItemId}", id);
            throw;
        }
    }

    public async Task<CartItem?> GetByCustomerAndProductAsync(string customerId, Guid productId)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT * FROM cart_items WHERE customer_id = @CustomerId AND product_id = @ProductId",
                new { CustomerId = customerId, ProductId = productId },
                _transaction);

            if (result == null)
            {
                return null;
            }

            return MapToCartItem(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart item by customer {CustomerId} and product {ProductId}", customerId, productId);
            throw;
        }
    }

    public async Task<List<CartItem>> GetByCustomerIdAsync(string customerId)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var results = await connection.QueryAsync<dynamic>(
                "SELECT * FROM cart_items WHERE customer_id = @CustomerId ORDER BY created_at",
                new { CustomerId = customerId },
                _transaction);

            var cartItems = new List<CartItem>();
            foreach (var result in results)
            {
                var cartItem = MapToCartItem(result);
                if (cartItem != null)
                    cartItems.Add(cartItem);
            }
            return cartItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart items by customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<CartItem> AddAsync(CartItem entity)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var parameters = new
            {
                Id = entity.Id,
                CustomerId = entity.CustomerId,
                ProductId = entity.ProductId,
                Quantity = entity.Quantity,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };

            await connection.ExecuteAsync(@"
                INSERT INTO cart_items 
                    (id, customer_id, product_id, quantity, created_at, updated_at)
                VALUES 
                    (@Id, @CustomerId, @ProductId, @Quantity, @CreatedAt, @UpdatedAt)",
                parameters, _transaction);

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding cart item {CartItemId}", entity.Id);
            throw;
        }
    }

    public async Task UpdateAsync(CartItem entity)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var parameters = new
            {
                Id = entity.Id,
                Quantity = entity.Quantity,
                UpdatedAt = DateTime.UtcNow
            };

            await connection.ExecuteAsync(@"
                UPDATE cart_items 
                SET 
                    quantity = @Quantity,
                    updated_at = @UpdatedAt
                WHERE id = @Id",
                parameters, _transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cart item {CartItemId}", entity.Id);
            throw;
        }
    }

    public async Task DeleteAsync(CartItem entity)
    {
        try
        {
            var connection = await GetConnectionAsync();

            await connection.ExecuteAsync(
                "DELETE FROM cart_items WHERE id = @Id",
                new { entity.Id },
                _transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cart item {CartItemId}", entity.Id);
            throw;
        }
    }

    public async Task DeleteByCustomerIdAsync(string customerId)
    {
        try
        {
            var connection = await GetConnectionAsync();

            await connection.ExecuteAsync(
                "DELETE FROM cart_items WHERE customer_id = @CustomerId",
                new { CustomerId = customerId },
                _transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cart items by customer {CustomerId}", customerId);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(string customerId, Guid productId)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var exists = await connection.ExecuteScalarAsync<bool>(
                "SELECT EXISTS(SELECT 1 FROM cart_items WHERE customer_id = @CustomerId AND product_id = @ProductId)",
                new { CustomerId = customerId, ProductId = productId },
                _transaction);

            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if cart item exists for customer {CustomerId} and product {ProductId}", customerId, productId);
            throw;
        }
    }

    public async Task<int> GetItemCountAsync(string customerId)
    {
        try
        {
            var connection = await GetConnectionAsync();

            var count = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM cart_items WHERE customer_id = @CustomerId",
                new { CustomerId = customerId },
                _transaction);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cart item count for customer {CustomerId}", customerId);
            throw;
        }
    }

    private CartItem? MapToCartItem(dynamic result)
    {
        if (result == null)
        {
            _logger.LogWarning("MapToCartItem: result is null");
            return null;
        }

        try
        {
            var cartItem = new CartItem
            {
                Id = result.id,
                CustomerId = result.customer_id,
                ProductId = result.product_id,
                Quantity = result.quantity,
                CreatedAt = result.created_at,
                UpdatedAt = result.updated_at
            };

            return cartItem;
        }
        catch (Exception ex)
        {
            LoggerExtensions.LogError(_logger, ex, "Error mapping CartItem from result");
            throw;
        }
    }
}

