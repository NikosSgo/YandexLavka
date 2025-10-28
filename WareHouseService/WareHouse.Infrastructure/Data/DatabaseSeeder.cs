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
                CreateStorageUnit("Wine Glass", "DISH-001", 100, "B-01-01", "Dishes", 4)
            };

            await context.StorageUnits.AddRangeAsync(storageUnits);
            await context.SaveChangesAsync();
        }
    }

    private static StorageUnit CreateStorageUnit(string name, string sku, int quantity, string location, string zone, int index)
    {
        // Простой и понятный способ
        var guidString = $"0000000{index}-0000-0000-0000-00000000000{index}";
        return new StorageUnit(Guid.Parse(guidString), name, sku, quantity, location, zone);
    }
}