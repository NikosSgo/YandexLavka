namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class CreateUserCommand : IRequest<Result<UserDto>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

