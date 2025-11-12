namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;

public class DeleteUserCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}

