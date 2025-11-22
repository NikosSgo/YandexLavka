using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Npgsql;

namespace WareHouse.Migrations;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            // Проверяем, если первый аргумент --seed, то запускаем заполнение данных
            if (args.Length > 0 && args[0] == "--seed")
            {
                Console.WriteLine("🌱 Starting database seeding...");

                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
                Console.WriteLine($"Environment: {environmentName}");

                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: false)
                    .AddEnvironmentVariables()
                    .Build();

                var connectionString = config.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
                }

                Console.WriteLine($"Database: {connectionString.Split(';').First(s => s.StartsWith("Database=")).Replace("Database=", "")}");

                var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), "seed_data.sql");
                
                if (!File.Exists(sqlFilePath))
                {
                    throw new FileNotFoundException($"SQL file not found: {sqlFilePath}");
                }

                var sqlScript = File.ReadAllText(sqlFilePath);
                
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine("Executing SQL script...");
                    
                    using var cmd = new NpgsqlCommand(sqlScript, connection);
                    cmd.CommandTimeout = 120;
                    var rowsAffected = cmd.ExecuteNonQuery();
                    
                    Console.WriteLine($"✅ SQL script executed successfully!");
                    Console.WriteLine($"Total rows affected: {rowsAffected}");
                }

                Console.WriteLine("✅ Database seeding completed successfully!");
                return;
            }

            // Иначе запускаем миграции
            Console.WriteLine("🚀 Starting database migrations...");

            var envName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine($"Environment: {envName}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{envName}.json", optional: false)
                .AddEnvironmentVariables() // ✅ Теперь будет работать
                .Build();

            var connString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
            }

            Console.WriteLine($"Database: {connString.Split(';').First(s => s.StartsWith("Database=")).Replace("Database=", "")}");

            var migrationRunner = new MigratorRunner(connString);
            migrationRunner.Migrate();

            Console.WriteLine("✅ Database migrations completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Operation failed: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Environment.Exit(1);
        }
    }
}