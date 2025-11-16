using AuthService.Application.Common;
using AuthService.Application.Dto;
using MediatR;

namespace AuthService.Application.Commands;

public class RegisterAccountCommand : IRequest<Result<TokenDto>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}


