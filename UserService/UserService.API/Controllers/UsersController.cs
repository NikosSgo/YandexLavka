namespace UserService.API.Controllers;

using Microsoft.AspNetCore.Mvc;
using UserService.Application.Common;
using UserService.Application.Users.Commands;
using UserService.Application.Users.Dto;
using UserService.Application.Users.Queries;
using MediatR;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IMediator mediator, ILogger<UsersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Получить всех пользователей
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        var query = new GetAllUsersQuery();
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            _logger.LogError("Failed to get users: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить пользователя по ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserById(Guid id)
    {
        var query = new GetUserByIdQuery { UserId = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get user {UserId}: {Error}", id, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить пользователя по телефону
    /// </summary>
    [HttpGet("by-phone/{phone}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserByPhone(string phone)
    {
        var query = new GetUserByPhoneQuery { Phone = phone };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get user by phone {Phone}: {Error}", phone, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить пользователя по email
    /// </summary>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery { Email = email };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get user by email {Email}: {Error}", email, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Создать нового пользователя
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("already exists") == true)
            {
                return Conflict(new { error = result.Error });
            }

            _logger.LogError("Failed to create user: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetUserById), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Обновить пользователя
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        command.UserId = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            if (result.Error?.Contains("already exists") == true)
            {
                return Conflict(new { error = result.Error });
            }

            _logger.LogError("Failed to update user {UserId}: {Error}", id, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Изменить телефон пользователя
    /// </summary>
    [HttpPatch("{id}/phone")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> ChangePhone(Guid id, [FromBody] ChangePhoneCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        command.UserId = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            if (result.Error?.Contains("already exists") == true)
            {
                return Conflict(new { error = result.Error });
            }

            _logger.LogError("Failed to change phone for user {UserId}: {Error}", id, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Изменить email пользователя
    /// </summary>
    [HttpPatch("{id}/email")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> ChangeEmail(Guid id, [FromBody] ChangeEmailCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        command.UserId = id;
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            if (result.Error?.Contains("already exists") == true)
            {
                return Conflict(new { error = result.Error });
            }

            _logger.LogError("Failed to change email for user {UserId}: {Error}", id, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Удалить пользователя
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var command = new DeleteUserCommand { UserId = id };
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to delete user {UserId}: {Error}", id, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return NoContent();
    }
}

