using MediatR;
using WareHouse.Application.DTOs;
using WareHouse.Application.Interfaces;
using WareHouse.Application.Queries;

namespace WareHouse.Application.Services;

public class OrderService : IOrderService
{
    private readonly IMediator _mediator;

    public OrderService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<OrderDto> GetOrderAsync(Guid orderId)
    {
        return await _mediator.Send(new GetOrderQuery(orderId));
    }

    public async Task<List<OrderDto>> GetOrdersByStatusAsync(string status)
    {
        return await _mediator.Send(new GetOrdersByStatusQuery(status));
    }

    public async Task<List<OrderDto>> GetOrdersByPickerAsync(string pickerId)
    {
        // Для простоты используем прямой запрос к репозиторию
        // В реальном приложении здесь был бы отдельный Query
        throw new NotImplementedException();
    }
}