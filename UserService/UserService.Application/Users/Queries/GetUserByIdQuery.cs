namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}

