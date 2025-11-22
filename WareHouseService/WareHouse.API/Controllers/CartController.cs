using MediatR;
using Microsoft.AspNetCore.Mvc;
using WareHouse.Application.Commands;
using WareHouse.Application.DTOs;
using WareHouse.Application.Queries;

namespace WareHouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CartController> _logger;

    public CartController(IMediator mediator, ILogger<CartController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{customerId}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CartDto>> GetCart(string customerId)
    {
        _logger.LogInformation("Getting cart for customer {CustomerId}", customerId);

        var cart = await _mediator.Send(new GetCartQuery(customerId));
        return Ok(cart);
    }

    [HttpPost("{customerId}/items")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CartItemDto>> AddToCart(string customerId, [FromBody] AddToCartRequest request)
    {
        _logger.LogInformation("Adding product {ProductId} to cart for customer {CustomerId}", 
            request.ProductId, customerId);

        var command = new AddToCartCommand(customerId, request.ProductId, request.Quantity);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpPut("{customerId}/items/{cartItemId:guid}")]
    [ProducesResponseType(typeof(CartItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartItemDto>> UpdateCartItem(
        string customerId, 
        Guid cartItemId, 
        [FromBody] UpdateCartItemRequest request)
    {
        _logger.LogInformation("Updating cart item {CartItemId} for customer {CustomerId}", 
            cartItemId, customerId);

        var command = new UpdateCartItemCommand(customerId, cartItemId, request.Quantity);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpDelete("{customerId}/items/{cartItemId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFromCart(string customerId, Guid cartItemId)
    {
        _logger.LogInformation("Removing cart item {CartItemId} for customer {CustomerId}", 
            cartItemId, customerId);

        var command = new RemoveFromCartCommand(customerId, cartItemId);
        await _mediator.Send(command);

        return Ok(new { message = "Item removed from cart successfully" });
    }

    [HttpDelete("{customerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> ClearCart(string customerId)
    {
        _logger.LogInformation("Clearing cart for customer {CustomerId}", customerId);

        var command = new ClearCartCommand(customerId);
        await _mediator.Send(command);

        return Ok(new { message = "Cart cleared successfully" });
    }
}

