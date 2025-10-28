using MediatR;
using Microsoft.AspNetCore.Mvc;
using WareHouse.Application.Commands;
using WareHouse.Application.DTOs;
using WareHouse.Application.Queries;

namespace WareHouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StockController> _logger;

    public StockController(IMediator mediator, ILogger<StockController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("products/{productId:guid}")]
    [ProducesResponseType(typeof(StockLevelDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StockLevelDto>> GetStockLevel(Guid productId)
    {
        _logger.LogInformation("Getting stock level for product {ProductId}", productId);

        var stockLevel = await _mediator.Send(new GetStockLevelQuery(productId));
        return Ok(stockLevel);
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(List<StockLevelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StockLevelDto>>> GetLowStockItems()
    {
        _logger.LogInformation("Getting low stock items");

        // В реальном приложении здесь был бы отдельный query
        // Пока возвращаем пустой список для примера
        return Ok(new List<StockLevelDto>());
    }

    [HttpPut("products/{productId:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateStock(Guid productId, [FromBody] UpdateStockRequest request)
    {
        _logger.LogInformation("Updating stock for product {ProductId}", productId);

        var command = new UpdateStockCommand(productId, request.Quantity, request.Location, request.Operation);
        await _mediator.Send(command);

        return Ok(new { message = "Stock updated successfully" });
    }

    [HttpPost("products/{productId:guid}/restock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RestockProduct(Guid productId, [FromBody] RestockRequest request)
    {
        _logger.LogInformation("Restocking product {ProductId}", productId);

        var command = new UpdateStockCommand(productId, request.Quantity, request.Location, "restock");
        await _mediator.Send(command);

        return Ok(new { message = "Product restocked successfully" });
    }
}

public record UpdateStockRequest(int Quantity, string Location, string Operation);
public record RestockRequest(int Quantity, string Location);