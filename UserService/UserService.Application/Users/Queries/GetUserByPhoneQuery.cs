namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class GetUserByPhoneQuery : IRequest<Result<UserDto>>
{
    public string Phone { get; set; } = string.Empty;
}

