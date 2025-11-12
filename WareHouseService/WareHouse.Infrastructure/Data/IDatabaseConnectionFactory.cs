// WareHouse.Infrastructure/Data/IDatabaseConnectionFactory.cs
using Npgsql;

namespace WareHouse.Infrastructure.Data;

public interface IDatabaseConnectionFactory
{
    Task<NpgsqlConnection> CreateConnectionAsync();
}

public class DatabaseConnectionFactory : IDatabaseConnectionFactory
{
    private readonly string _connectionString;

    public DatabaseConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<NpgsqlConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }
}