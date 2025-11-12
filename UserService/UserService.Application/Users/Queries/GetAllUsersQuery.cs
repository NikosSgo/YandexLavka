namespace UserService.Application.Users.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class GetAllUsersQuery : IRequest<Result<List<UserDto>>>
{
}

