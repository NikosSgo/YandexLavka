namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class GetUserByEmailQuery : IRequest<Result<UserDto>>
{
    public string Email { get; set; } = string.Empty;
}

