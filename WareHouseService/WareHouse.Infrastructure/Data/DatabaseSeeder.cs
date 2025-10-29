// WareHouse.Infrastructure/Data/DatabaseSeeder.cs
using Dapper;
using Npgsql;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(string connectionString)
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        try
        {
            // ✅ ПРОВЕРЯЕМ СТРУКТУРУ ТАБЛИЦЫ И ДОБАВЛЯЕМ НЕДОСТАЮЩИЕ КОЛОНКИ
            await EnsureTableStructureAsync(connection);

            // Проверяем есть ли уже данные
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM storage_units");
            if (count > 0)
            {
                Console.WriteLine("ℹ️ Database already contains data, skipping seeding");
                return;
            }

            Console.WriteLine("🌱 Seeding database with test data...");

            // ✅ ФИКС: Используем одинаковые ProductId для одинаковых продуктов
            var milkProductId = Guid.NewGuid();
            var waterProductId = Guid.NewGuid();
            var applesProductId = Guid.NewGuid();
            var breadProductId = Guid.NewGuid();
            var eggsProductId = Guid.NewGuid();

            var storageUnits = new List<object>
            {
                new {
                    Id = Guid.NewGuid(),
                    ProductId = milkProductId,
                    ProductName = "Milk",
                    Sku = "MILK-001",
                    Quantity = 50,
                    ReservedQuantity = 5,
                    Location = "A-01-01",
                    Zone = "A",
                    LastRestocked = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    IsLowStock = false,
                    IsOutOfStock = false
                },
                new {
                    Id = Guid.NewGuid(),
                    ProductId = waterProductId,
                    ProductName = "Water",
                    Sku = "WATER-001",
                    Quantity = 25,
                    ReservedQuantity = 3,
                    Location = "B-02-01",
                    Zone = "B",
                    LastRestocked = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    IsLowStock = false,
                    IsOutOfStock = false
                },
                new {
                    Id = Guid.NewGuid(),
                    ProductId = applesProductId,
                    ProductName = "Apples",
                    Sku = "FRUIT-001",
                    Quantity = 8,
                    ReservedQuantity = 2,
                    Location = "C-03-01",
                    Zone = "C",
                    LastRestocked = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = (DateTime?)null,
                    IsLowStock = true,
                    IsOutOfStock = false
                },
                new {
                    Id = Guid.NewGuid(),
                    ProductId = breadProductId,
                    ProductName = "Bread",
                    Sku = "BREAD-001",
                    Quantity = 15,
                    ReservedQuantity = 0,
                    Location = "A-01-02",
                    Zone = "A",
                    LastRestocked = DateTime.UtcNow.AddDays(-1),
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow,
                    IsLowStock = false,
                    IsOutOfStock = false
                },
                new {
                    Id = Guid.NewGuid(),
                    ProductId = eggsProductId,
                    ProductName = "Eggs",
                    Sku = "EGGS-001",
                    Quantity = 0,
                    ReservedQuantity = 0,
                    Location = "B-02-02",
                    Zone = "B",
                    LastRestocked = DateTime.UtcNow.AddDays(-5),
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow,
                    IsLowStock = false,
                    IsOutOfStock = true
                }
            };

            foreach (var unit in storageUnits)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO storage_units (id, product_id, product_name, sku, quantity, reserved_quantity,
                                             location, zone, last_restocked, ""CreatedAt"", ""UpdatedAt"", 
                                             ""IsLowStock"", ""IsOutOfStock"")
                    VALUES (@Id, @ProductId, @ProductName, @Sku, @Quantity, @ReservedQuantity,
                           @Location, @Zone, @LastRestocked, @CreatedAt, @UpdatedAt, 
                           @IsLowStock, @IsOutOfStock)", unit);
            }

            // ✅ ДОБАВЛЯЕМ ТЕСТОВЫЕ ЗАКАЗЫ И PICKING TASKS (используем те же ProductId)
            await SeedTestOrdersAndTasksAsync(connection, milkProductId, breadProductId);

            Console.WriteLine("✅ Test data seeded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during database seeding: {ex.Message}");
            throw;
        }
    }

    private static async Task EnsureTableStructureAsync(NpgsqlConnection connection)
    {
        Console.WriteLine("🔍 Checking database structure...");

        // Проверяем существование таблицы storage_units
        var tableExists = await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_name = 'storage_units')");

        if (!tableExists)
        {
            Console.WriteLine("❌ Table 'storage_units' does not exist. Please run migrations first.");
            throw new InvalidOperationException("Table 'storage_units' does not exist. Run migrations first.");
        }

        Console.WriteLine("✅ Database structure is valid - all tables exist");

        // ❌ УБИРАЕМ попытки создания колонок - они уже существуют
        // Просто логируем что колонки на месте
        var existingColumns = await connection.QueryAsync<string>(@"
        SELECT column_name 
        FROM information_schema.columns 
        WHERE table_name = 'storage_units' 
        AND column_name IN ('CreatedAt', 'UpdatedAt', 'IsLowStock', 'IsOutOfStock')");

        Console.WriteLine($"✅ Found {existingColumns.Count()} audit columns in storage_units");
    }

    private static async Task SeedTestOrdersAndTasksAsync(NpgsqlConnection connection, Guid milkProductId, Guid breadProductId)
    {
        // Создаем тестовые заказы
        var testOrderId = Guid.NewGuid();
        var testCustomerId = "customer-123";

        // Создаем заказ
        await connection.ExecuteAsync(@"
            INSERT INTO orders (id, customer_id, status, created_at, picking_started_at, picking_completed_at, packing_completed_at)
            VALUES (@Id, @CustomerId, @Status, @CreatedAt, @PickingStartedAt, @PickingCompletedAt, @PackingCompletedAt)",
            new
            {
                Id = testOrderId,
                CustomerId = testCustomerId,
                Status = "Received",
                CreatedAt = DateTime.UtcNow,
                PickingStartedAt = (DateTime?)null,
                PickingCompletedAt = (DateTime?)null,
                PackingCompletedAt = (DateTime?)null
            });

        // Создаем линии заказа (используем те же ProductId что и в storage_units)
        var orderLines = new[]
        {
            new { OrderId = testOrderId, ProductId = milkProductId, ProductName = "Milk", Sku = "MILK-001", QuantityOrdered = 2, QuantityPicked = 0, UnitPrice = 2.50m },
            new { OrderId = testOrderId, ProductId = breadProductId, ProductName = "Bread", Sku = "BREAD-001", QuantityOrdered = 1, QuantityPicked = 0, UnitPrice = 1.20m }
        };

        foreach (var line in orderLines)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO order_lines (order_id, product_id, product_name, sku, quantity_ordered, quantity_picked, unit_price)
                VALUES (@OrderId, @ProductId, @ProductName, @Sku, @QuantityOrdered, @QuantityPicked, @UnitPrice)",
                line);
        }

        // Создаем тестовое задание на сборку
        var pickingTaskId = Guid.NewGuid();
        await connection.ExecuteAsync(@"
            INSERT INTO picking_tasks (id, order_id, assigned_picker, status, zone, created_at, started_at, completed_at)
            VALUES (@Id, @OrderId, @AssignedPicker, @Status, @Zone, @CreatedAt, @StartedAt, @CompletedAt)",
            new
            {
                Id = pickingTaskId,
                OrderId = testOrderId,
                AssignedPicker = "picker-john",
                Status = "Created",
                Zone = "A",
                CreatedAt = DateTime.UtcNow,
                StartedAt = (DateTime?)null,
                CompletedAt = (DateTime?)null
            });

        // Создаем элементы для задания на сборку (используем те же ProductId)
        var pickingItems = new[]
        {
            new { PickingTaskId = pickingTaskId, ProductId = milkProductId, ProductName = "Milk", Sku = "MILK-001", Quantity = 2, StorageLocation = "A-01-01", Barcode = "BC-MILK-001" },
            new { PickingTaskId = pickingTaskId, ProductId = breadProductId, ProductName = "Bread", Sku = "BREAD-001", Quantity = 1, StorageLocation = "A-01-02", Barcode = "BC-BREAD-001" }
        };

        foreach (var item in pickingItems)
        {
            await connection.ExecuteAsync(@"
                INSERT INTO picking_items (picking_task_id, product_id, product_name, sku, quantity, storage_location, barcode)
                VALUES (@PickingTaskId, @ProductId, @ProductName, @Sku, @Quantity, @StorageLocation, @Barcode)",
                item);
        }

        Console.WriteLine("✅ Test orders and picking tasks seeded successfully");
    }
}