using WareHouse.Application.DTOs;

namespace WareHouse.Application.Interfaces;

public interface IOrderService
{
    Task<OrderDto> GetOrderAsync(Guid orderId);
    Task<List<OrderDto>> GetOrdersByStatusAsync(string status);
    Task<List<OrderDto>> GetOrdersByPickerAsync(string pickerId);
}

public interface IPickingService
{
    Task<PickingTaskDto> StartPickingAsync(Guid orderId, string pickerId, string zone);
    Task CompletePickingAsync(Guid orderId, Dictionary<Guid, int> pickedQuantities);
    Task CancelPickingAsync(Guid orderId, string reason);
    Task<List<PickingTaskDto>> GetPickingTasksAsync(string status, string zone);
}

public interface IStockService
{
    Task<StockLevelDto> GetStockLevelAsync(Guid productId);
    Task<List<StockLevelDto>> GetLowStockItemsAsync();
    Task UpdateStockAsync(StockUpdateRequest request);
}