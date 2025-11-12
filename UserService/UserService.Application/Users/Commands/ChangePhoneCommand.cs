namespace UserService.Application.Users.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Users.Dto;

public class ChangePhoneCommand : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
    public string Phone { get; set; } = string.Empty;
}

