using Microsoft.AspNetCore.Mvc;
using OrderService.API.Contracts;
using OrderService.Application.Abstractions;
using OrderService.Application.Contracts;
using OrderService.Domain.Enums;

namespace OrderService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderApplicationService _orderService;
    private readonly IOrderStateMachine _stateMachine;

    public OrdersController(
        IOrderApplicationService orderService,
        IOrderStateMachine stateMachine)
    {
        _orderService = orderService;
        _stateMachine = stateMachine;
    }

    [HttpPost]
    public async Task<ActionResult<OrderDetailsDto>> CreateAsync(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var command = Map(request);
        var result = await _orderService.CreateAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { orderId = result.Id }, result);
    }

    [HttpGet("{orderId:guid}")]
    public async Task<ActionResult<OrderDetailsDto>> GetByIdAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetAsync(orderId, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPost("{orderId:guid}/advance")]
    public async Task<ActionResult<OrderDetailsDto>> AdvanceStatusAsync(
        Guid orderId,
        [FromBody] AdvanceOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AdvanceOrderStatusCommand(
            orderId,
            request.TargetStatus,
            request.Actor,
            request.Notes);

        try
        {
            var result = await _orderService.AdvanceStatusAsync(command, cancellationToken);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{orderId:guid}/next-statuses")]
    public async Task<ActionResult<IReadOnlyCollection<OrderStatus>>> GetNextStatusesAsync(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await _orderService.GetAsync(orderId, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        var statuses = _stateMachine.GetNextStatuses(order.Status);
        return Ok(statuses);
    }

    private static CreateOrderCommand Map(CreateOrderRequest request)
    {
        var address = new AddressDto(
            request.DeliveryAddress.Country,
            request.DeliveryAddress.City,
            request.DeliveryAddress.Street,
            request.DeliveryAddress.Building,
            request.DeliveryAddress.Apartment,
            request.DeliveryAddress.Comment);

        var items = request.Items
            .Select(i => new OrderItemDto(i.Sku, i.Name, i.Price, i.Quantity))
            .ToArray();

        return new CreateOrderCommand(
            request.UserId,
            address,
            items,
            request.Metadata);
    }
}

