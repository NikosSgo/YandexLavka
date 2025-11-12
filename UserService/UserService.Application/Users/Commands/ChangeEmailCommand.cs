namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class ChangeEmailCommand : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}

