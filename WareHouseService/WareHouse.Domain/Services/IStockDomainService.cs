using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Services
{
    public interface IStockDomainService
    {
        Task<bool> IsProductAvailableAsync(Guid productId, int quantity);
        Task<List<StorageUnit>> FindOptimalStorageUnitsAsync(Guid productId, int quantity);
        Task<int> GetTotalAvailableQuantityAsync(Guid productId);
        Task ReserveStockForOrderAsync(Guid orderId);
        Task ReleaseStockReservationAsync(Guid orderId);
    }
}