using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WareHouse.Domain.Entities;

using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Interfaces;

public interface IStorageUnitRepository : IRepository<StorageUnit>
{
    Task<StorageUnit> GetByIdAsync(Guid unitId);
    Task<StorageUnit> GetByProductAndLocationAsync(Guid productId, string location);
    Task<List<StorageUnit>> GetByProductAsync(Guid productId);
    Task<List<StorageUnit>> GetByLocationAsync(string location);
    Task<List<StorageUnit>> GetByZoneAsync(string zone);
    Task<List<StorageUnit>> GetUnitsForOrderAsync(Guid orderId);
    Task<List<StorageUnit>> GetLowStockUnitsAsync();
}