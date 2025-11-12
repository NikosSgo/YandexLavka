namespace UserService.Application.Addresses.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;

public class GetAddressByIdQuery : IRequest<Result<AddressDto>>
{
    public Guid AddressId { get; set; }
    public Guid UserId { get; set; }
}

