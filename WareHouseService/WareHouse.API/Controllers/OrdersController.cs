using MediatR;
using Microsoft.AspNetCore.Mvc;
using WareHouse.Application.Commands;
using WareHouse.Application.DTOs;
using WareHouse.Application.Queries;

namespace WareHouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
    {
        _logger.LogInformation("Getting order {OrderId}", orderId);

        var order = await _mediator.Send(new GetOrderQuery(orderId));
        return Ok(order);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersByStatus([FromQuery] string status)
    {
        _logger.LogInformation("Getting orders with status {Status}", status);

        var orders = await _mediator.Send(new GetOrdersByStatusQuery(status));
        return Ok(orders);
    }

    [HttpPost("{orderId:guid}/start-picking")]
    [ProducesResponseType(typeof(PickingTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PickingTaskDto>> StartPicking(Guid orderId, [FromBody] StartPickingRequest request)
    {
        _logger.LogInformation("Starting picking for order {OrderId}", orderId);

        var command = new StartPickingCommand(orderId, request.PickerId, request.Zone);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPost("{orderId:guid}/complete-picking")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CompletePicking(Guid orderId, [FromBody] CompletePickingRequest request)
    {
        _logger.LogInformation("Completing picking for order {OrderId}", orderId);

        var command = new CompletePickingCommand(orderId, request.PickedQuantities);
        await _mediator.Send(command);

        return Ok(new { message = "Picking completed successfully" });
    }

    [HttpPost("{orderId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CancelOrder(Guid orderId, [FromBody] CancelOrderRequest request)
    {
        _logger.LogInformation("Cancelling order {OrderId}", orderId);

        var command = new CancelPickingCommand(orderId, request.Reason);
        await _mediator.Send(command);

        return Ok(new { message = "Order cancelled successfully" });
    }
}

public record StartPickingRequest(string PickerId, string Zone);
public record CompletePickingRequest(Dictionary<Guid, int> PickedQuantities);
public record CancelOrderRequest(string Reason);