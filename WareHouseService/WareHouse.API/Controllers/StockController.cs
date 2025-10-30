using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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
    public async Task<ActionResult<StockLevelDto>> GetStockLevel(
        [FromRoute]
        [Example("3f9bcc41-4a35-496b-a008-30428253ecb4")] // ✅ ДОБАВЬТЕ ЭТОТ АТРИБУТ
        Guid productId)
    {
        _logger.LogInformation("Getting stock level for product {ProductId}", productId);

        try
        {
            var stockLevel = await _mediator.Send(new GetStockLevelQuery(productId));
            return Ok(stockLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock level for product {ProductId}", productId);
            return NotFound(new { message = $"Stock level not found for product {productId}" });
        }
    }

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(List<StockLevelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<StockLevelDto>>> GetLowStockItems()
    {
        _logger.LogInformation("Getting low stock items");

        try
        {
            // ✅ РЕАЛЬНАЯ ЛОГИКА ДЛЯ ПОЛУЧЕНИЯ ТОВАРОВ С НИЗКИМ ЗАПАСОМ
            var query = new GetLowStockQuery();
            var lowStockItems = await _mediator.Send(query);
            return Ok(lowStockItems);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock items");
            return StatusCode(500, new { message = "Error retrieving low stock items" });
        }
    }

    [HttpPut("products/{productId:guid}/stock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> UpdateStock(
        [FromRoute]
        [Example("3f9bcc41-4a35-496b-a008-30428253ecb4")] // ✅ ДОБАВЬТЕ ЭТОТ АТРИБУТ
        Guid productId,
        [FromBody] UpdateStockRequest request)
    {
        _logger.LogInformation("Updating stock for product {ProductId}", productId);

        try
        {
            var command = new UpdateStockCommand(productId, request.Quantity, request.Location, request.Operation);
            await _mediator.Send(command);

            return Ok(new
            {
                message = "Stock updated successfully",
                productId = productId,
                operation = request.Operation
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("products/{productId:guid}/restock")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> RestockProduct(
        [FromRoute]
        [Example("3f9bcc41-4a35-496b-a008-30428253ecb4")] // ✅ ДОБАВЬТЕ ЭТОТ АТРИБУТ
        Guid productId,
        [FromBody] RestockRequest request)
    {
        _logger.LogInformation("Restocking product {ProductId}", productId);

        try
        {
            var command = new UpdateStockCommand(productId, request.Quantity, request.Location, "restock");
            await _mediator.Send(command);

            return Ok(new
            {
                message = "Product restocked successfully",
                productId = productId,
                quantity = request.Quantity,
                location = request.Location
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product {ProductId}", productId);
            return BadRequest(new { message = ex.Message });
        }
    }
}

// ✅ ДОБАВЬТЕ КЛАСС ДЛЯ ПРИМЕРА GUID
public class ExampleAttribute : Attribute
{
    public string Value { get; }

    public ExampleAttribute(string value)
    {
        Value = value;
    }
}

public record UpdateStockRequest(
    [Required][Range(1, int.MaxValue)] int Quantity,
    [Required] string Location,
    [Required] string Operation
);

public record RestockRequest(
    [Required][Range(1, int.MaxValue)] int Quantity,
    [Required] string Location
);