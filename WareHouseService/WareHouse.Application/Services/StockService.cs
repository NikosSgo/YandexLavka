using MediatR;
using WareHouse.Application.Commands;
using WareHouse.Application.DTOs;
using WareHouse.Application.Interfaces;
using WareHouse.Application.Queries;

namespace WareHouse.Application.Services;

public class StockService : IStockService
{
    private readonly IMediator _mediator;

    public StockService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<StockLevelDto> GetStockLevelAsync(Guid productId)
    {
        return await _mediator.Send(new GetStockLevelQuery(productId));
    }

    public async Task<List<StockLevelDto>> GetLowStockItemsAsync()
    {
        // Для простоты используем прямую работу с репозиторием
        // В реальном приложении здесь был бы отдельный Query
        throw new NotImplementedException();
    }

    public async Task UpdateStockAsync(StockUpdateRequest request)
    {
        await _mediator.Send(new UpdateStockCommand(
            request.ProductId,
            request.Quantity,
            request.Location,
            request.Operation
        ));
    }
}