// WareHouse.Domain/Interfaces/IStorageUnitRepository.cs
using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Interfaces;

public interface IStorageUnitRepository
{
    Task<StorageUnit> GetByIdAsync(Guid unitId);
    Task<StorageUnit> GetByProductAndLocationAsync(Guid productId, string location);
    Task<List<StorageUnit>> GetByProductAsync(Guid productId);
    Task<List<StorageUnit>> GetByLocationAsync(string location);
    Task<List<StorageUnit>> GetByZoneAsync(string zone);
    Task<List<StorageUnit>> GetUnitsForOrderAsync(Guid orderId);
    Task<List<StorageUnit>> GetLowStockUnitsAsync();

    // IRepository methods
    Task AddAsync(StorageUnit entity);
    Task UpdateAsync(StorageUnit entity);
    Task DeleteAsync(StorageUnit entity);
    Task<IReadOnlyList<StorageUnit>> GetAllAsync();
}