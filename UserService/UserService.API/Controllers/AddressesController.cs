namespace UserService.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using UserService.Application.Addresses.Commands;
using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Queries;
using MediatR;

[ApiController]
[Route("api/users/{userId}/addresses")]
[Produces("application/json")]
public class AddressesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AddressesController> _logger;

    public AddressesController(IMediator mediator, ILogger<AddressesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Получить все адреса пользователя
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AddressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<AddressDto>>> GetAddressesByUserId(Guid userId)
    {
        var query = new GetAddressesByUserIdQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get addresses for user {UserId}: {Error}", userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить адрес по ID
    /// </summary>
    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> GetAddressById(Guid userId, Guid addressId)
    {
        var query = new GetAddressByIdQuery { UserId = userId, AddressId = addressId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get address {AddressId} for user {UserId}: {Error}", addressId, userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить основной адрес пользователя
    /// </summary>
    [HttpGet("primary")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> GetPrimaryAddress(Guid userId)
    {
        var query = new GetPrimaryAddressQuery { UserId = userId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get primary address for user {UserId}: {Error}", userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        if (result.Value == null)
        {
            return NotFound(new { error = "Primary address not found" });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Добавить адрес пользователю
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> AddAddress(Guid userId, [FromBody] AddAddressCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        command.UserId = userId;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            if (result.Error?.Contains("Maximum number") == true || result.Error?.Contains("reached") == true)
            {
                return Conflict(new { error = result.Error });
            }

            _logger.LogError("Failed to add address for user {UserId}: {Error}", userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return CreatedAtAction(
            nameof(GetAddressById),
            new { userId = userId, addressId = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Обновить адрес
    /// </summary>
    [HttpPut("{addressId}")]
    [ProducesResponseType(typeof(AddressDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AddressDto>> UpdateAddress(
        Guid userId,
        Guid addressId,
        [FromBody] UpdateAddressCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        command.UserId = userId;
        command.AddressId = addressId;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to update address {AddressId} for user {UserId}: {Error}", addressId, userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Удалить адрес
    /// </summary>
    [HttpDelete("{addressId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveAddress(Guid userId, Guid addressId)
    {
        var command = new RemoveAddressCommand { UserId = userId, AddressId = addressId };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to remove address {AddressId} for user {UserId}: {Error}", addressId, userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Установить адрес как основной
    /// </summary>
    [HttpPatch("{addressId}/set-primary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SetPrimaryAddress(Guid userId, Guid addressId)
    {
        var command = new SetPrimaryAddressCommand { UserId = userId, AddressId = addressId };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to set primary address {AddressId} for user {UserId}: {Error}", addressId, userId, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return NoContent();
    }
}

