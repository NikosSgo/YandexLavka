using Microsoft.EntityFrameworkCore;
using WareHouse.Domain.Entities;

namespace WareHouse.Infrastructure.Data;

public class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (!await context.StorageUnits.AnyAsync())
        {
            var storageUnits = new List<StorageUnit>
            {
                CreateStorageUnit("Milk", "MILK-001", 50, "A-01-01", "Dairy", 1),
                CreateStorageUnit("Water", "WATER-001", 30, "A-01-02", "Beverages", 2),
                CreateStorageUnit("Apples", "FRUIT-001", 20, "A-02-01", "Fruits", 3),
                CreateStorageUnit("Wine Glass", "DISH-001", 100, "B-01-01", "Dishes", 4),
                CreateStorageUnit("Bread", "BAKERY-001", 40, "A-03-01", "Bakery", 5),
                CreateStorageUnit("Cheese", "DAIRY-002", 25, "A-01-03", "Dairy", 6)
            };

            await context.StorageUnits.AddRangeAsync(storageUnits);
            await context.SaveChangesAsync();
        }

        if (!await context.Orders.AnyAsync())
        {
            var orders = new List<OrderAggregate>
            {
                new OrderAggregate(
                    Guid.Parse("10000001-0000-0000-0000-000000000001"),
                    "customer-001",
                    new List<OrderLine>
                    {
                        new OrderLine(
                            Guid.Parse("10000001-0000-0000-0000-000000000001"), // OrderId
                            Guid.Parse("00000001-0000-0000-0000-000000000001"), // ProductId (Milk)
                            "Milk",
                            "MILK-001",
                            2,
                            80.50m
                        ),
                        new OrderLine(
                            Guid.Parse("10000001-0000-0000-0000-000000000001"), // OrderId
                            Guid.Parse("00000002-0000-0000-0000-000000000002"), // ProductId (Water)
                            "Water",
                            "WATER-001",
                            1,
                            40.00m
                        )
                    }
                ),
                new OrderAggregate(
                    Guid.Parse("10000002-0000-0000-0000-000000000002"),
                    "customer-002",
                    new List<OrderLine>
                    {
                        new OrderLine(
                            Guid.Parse("10000002-0000-0000-0000-000000000002"), // OrderId
                            Guid.Parse("00000003-0000-0000-0000-000000000003"), // ProductId (Apples)
                            "Apples",
                            "FRUIT-001",
                            3,
                            120.00m
                        ),
                        new OrderLine(
                            Guid.Parse("10000002-0000-0000-0000-000000000002"), // OrderId
                            Guid.Parse("00000004-0000-0000-0000-000000000004"), // ProductId (Wine Glass)
                            "Wine Glass",
                            "DISH-001",
                            2,
                            250.00m
                        )
                    }
                )
            };

            await context.Orders.AddRangeAsync(orders);
            await context.SaveChangesAsync();
        }
    }

    private static StorageUnit CreateStorageUnit(string name, string sku, int quantity, string location, string zone, int index)
    {
        var productId = Guid.Parse($"0000000{index}-0000-0000-0000-00000000000{index}");

        // StorageUnit теперь автоматически генерирует Id через базовый конструктор
        return new StorageUnit(productId, name, sku, quantity, location, zone);
    }
}