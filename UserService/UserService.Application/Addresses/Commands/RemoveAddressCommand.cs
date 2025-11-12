namespace UserService.Application.Addresses.Commands;

using MediatR;
using UserService.Application.Common;

public class RemoveAddressCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
    public Guid AddressId { get; set; }
}

