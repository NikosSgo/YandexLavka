using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

namespace WareHouse.Migrations;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("🚀 Starting database migrations...");

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            Console.WriteLine($"Environment: {environmentName}");

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: false)
                .AddEnvironmentVariables() // ✅ Теперь будет работать
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found");
            }

            Console.WriteLine($"Database: {connectionString.Split(';').First(s => s.StartsWith("Database=")).Replace("Database=", "")}");

            var migrationRunner = new MigratorRunner(connectionString);
            migrationRunner.Migrate();

            Console.WriteLine("✅ Database migrations completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Migration failed: {ex.Message}");
            Environment.Exit(1);
        }
    }
}