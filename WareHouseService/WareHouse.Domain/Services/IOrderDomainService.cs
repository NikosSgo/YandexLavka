using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Services
{
    public interface IOrderDomainService
    {
        Task<bool> CanStartPickingAsync(Guid orderId);
        Task<bool> ValidatePickingQuantitiesAsync(Guid orderId, Dictionary<Guid, int> pickedQuantities);
        Task<List<StorageUnit>> SuggestPickingLocationsAsync(Guid orderId);
        Task<decimal> CalculatePickingTimeEstimateAsync(Guid orderId);
    }
}