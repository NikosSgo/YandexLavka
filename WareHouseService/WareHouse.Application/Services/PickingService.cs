using MediatR;
using WareHouse.Application.Commands;
using WareHouse.Application.DTOs;
using WareHouse.Application.Interfaces;
using WareHouse.Application.Queries;

namespace WareHouse.Application.Services;

public class PickingService : IPickingService
{
    private readonly IMediator _mediator;

    public PickingService(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<PickingTaskDto> StartPickingAsync(Guid orderId, string pickerId, string zone)
    {
        return await _mediator.Send(new StartPickingCommand(orderId, pickerId, zone));
    }

    public async Task CompletePickingAsync(Guid orderId, Dictionary<Guid, int> pickedQuantities)
    {
        await _mediator.Send(new CompletePickingCommand(orderId, pickedQuantities));
    }

    public async Task CancelPickingAsync(Guid orderId, string reason)
    {
        await _mediator.Send(new CancelPickingCommand(orderId, reason));
    }

    public async Task<List<PickingTaskDto>> GetPickingTasksAsync(string status, string zone)
    {
        return await _mediator.Send(new GetPickingTasksQuery(status, zone));
    }
}