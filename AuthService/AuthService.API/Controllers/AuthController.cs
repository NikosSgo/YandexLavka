using AuthService.Application.Commands;
using AuthService.Application.Common;
using AuthService.Application.Dto;
using AuthService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Зарегистрировать новый аккаунт
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TokenDto>> Register([FromBody] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new RegisterAccountCommand
        {
            Email = registerDto.Email,
            Password = registerDto.Password
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("already exists") == true)
            {
                return Conflict(new { error = result.Error });
            }

            _logger.LogError("Failed to register account: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return CreatedAtAction(nameof(GetAccount), new { id = result.Value!.Account.Id }, result.Value);
    }

    /// <summary>
    /// Войти в аккаунт
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TokenDto>> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new LoginCommand
        {
            Email = loginDto.Email,
            Password = loginDto.Password
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("Invalid") == true || result.Error?.Contains("deactivated") == true)
            {
                return Unauthorized(new { error = result.Error });
            }

            _logger.LogError("Failed to login: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Получить информацию об аккаунте по ID
    /// </summary>
    [HttpGet("account/{id}")]
    [ProducesResponseType(typeof(AccountDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccountDto>> GetAccount(Guid id)
    {
        var query = new GetAccountQuery { AccountId = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            if (result.Error?.Contains("not found") == true)
            {
                return NotFound(new { error = result.Error });
            }

            _logger.LogError("Failed to get account {AccountId}: {Error}", id, result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error });
        }

        return Ok(result.Value);
    }
}


