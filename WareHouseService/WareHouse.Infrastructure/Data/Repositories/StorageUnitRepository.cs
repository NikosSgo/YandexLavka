using Microsoft.EntityFrameworkCore;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Infrastructure.Data.Repositories;

public class StorageUnitRepository : BaseRepository<StorageUnit>, IStorageUnitRepository
{
    public StorageUnitRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async override Task<StorageUnit> GetByIdAsync(Guid unitId)
    {
        return await _context.StorageUnits
            .FirstOrDefaultAsync(su => su.UnitId == unitId);
    }

    public async Task<StorageUnit> GetByProductAndLocationAsync(Guid productId, string location)
    {
        return await _context.StorageUnits
            .FirstOrDefaultAsync(su => su.ProductId == productId && su.Location == location);
    }

    public async Task<List<StorageUnit>> GetByProductAsync(Guid productId)
    {
        return await _context.StorageUnits
            .Where(su => su.ProductId == productId)
            .OrderBy(su => su.Location)
            .ToListAsync();
    }

    public async Task<List<StorageUnit>> GetByLocationAsync(string location)
    {
        return await _context.StorageUnits
            .Where(su => su.Location == location)
            .ToListAsync();
    }

    public async Task<List<StorageUnit>> GetByZoneAsync(string zone)
    {
        return await _context.StorageUnits
            .Where(su => su.Zone == zone)
            .OrderBy(su => su.Location)
            .ToListAsync();
    }

    public async Task<List<StorageUnit>> GetUnitsForOrderAsync(Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return new List<StorageUnit>();

        var productIds = order.Lines.Select(l => l.ProductId).ToList();

        return await _context.StorageUnits
            .Where(su => productIds.Contains(su.ProductId) && su.AvailableQuantity > 0)
            .OrderBy(su => su.Zone)
            .ThenBy(su => su.Location)
            .ToListAsync();
    }

    public async Task<List<StorageUnit>> GetLowStockUnitsAsync()
    {
        return await _context.StorageUnits
            .Where(su => su.AvailableQuantity <= 10) // Меньше или равно 10 единиц
            .OrderBy(su => su.AvailableQuantity)
            .ToListAsync();
    }
}