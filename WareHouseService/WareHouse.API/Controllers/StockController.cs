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
    [ProducesResponseType(typeof(ProductStockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductStockDto>> GetStockLevel(
        [FromRoute]
        [Example("3f9bcc41-4a35-496b-a008-30428253ecb4")]
        Guid productId)
    {
        _logger.LogInformation("Getting stock level for product {ProductId}", productId);

        try
        {
            var stockLevel = await _mediator.Send(new GetProductStockQuery(productId));
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

    [HttpPost("products")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        _logger.LogInformation("Creating new product with SKU {Sku}", request.Sku);

        try
        {
            var command = new CreateProductCommand(
                request.Name,
                request.Sku,
                request.Description,
                request.Category,
                request.UnitPrice,
                request.WeightKg,
                request.RequiresRefrigeration
            );

            var product = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetStockLevel),
                new { productId = product.Id },
                product);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict creating product with SKU {Sku}", request.Sku);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with SKU {Sku}", request.Sku);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("products/{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteProduct(
        [FromRoute]
        [Example("3f9bcc41-4a35-496b-a008-30428253ecb4")]
        Guid productId)
    {
        _logger.LogInformation("Deleting product {ProductId}", productId);

        try
        {
            var command = new DeleteProductCommand(productId);
            await _mediator.Send(command);

            return Ok(new
            {
                message = "Product deleted successfully",
                productId = productId
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product {ProductId} not found", productId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete product {ProductId}: {Message}", productId, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("storage-units/create")]
    [ProducesResponseType(typeof(StorageUnitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<StorageUnitDto>> CreateStorageUnit([FromBody] CreateStorageUnitRequest request)
    {
        _logger.LogInformation("Creating storage unit for product {ProductId} at location {Location}",
            request.ProductId, request.Location);

        try
        {
            var command = new CreateStorageUnitCommand(
                request.ProductId,
                request.Location,
                request.Quantity,
                request.Zone
            );

            var storageUnit = await _mediator.Send(command);

            return CreatedAtAction(
                nameof(GetStockLevel),
                new { productId = storageUnit.ProductId },
                storageUnit);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product {ProductId} not found", request.ProductId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Conflict creating storage unit: {Message}", ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating storage unit for product {ProductId}", request.ProductId);
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("storage-units/{storageUnitId:guid}/delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> DeleteStorageUnit(
        [FromRoute]
        [Example("3f9bcc41-4a35-496b-a008-30428253ecb4")]
        Guid storageUnitId)
    {
        _logger.LogInformation("Deleting storage unit {StorageUnitId}", storageUnitId);

        try
        {
            var command = new DeleteStorageUnitCommand(storageUnitId);
            await _mediator.Send(command);

            return Ok(new
            {
                message = "Storage unit deleted successfully",
                storageUnitId = storageUnitId
            });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Storage unit {StorageUnitId} not found", storageUnitId);
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Cannot delete storage unit {StorageUnitId}: {Message}", storageUnitId, ex.Message);
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting storage unit {StorageUnitId}", storageUnitId);
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

public record CreateProductRequest(
    [Required] string Name,
    [Required] string Sku,
    string Description,
    [Required] string Category,
    [Required][Range(0.01, double.MaxValue)] decimal UnitPrice,
    decimal? WeightKg,
    bool RequiresRefrigeration
);

public record CreateStorageUnitRequest(
    [Required] Guid ProductId,
    [Required] string Location,
    [Required][Range(1, int.MaxValue)] int Quantity,
    string? Zone = null
);