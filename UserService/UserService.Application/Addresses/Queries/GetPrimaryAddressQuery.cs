namespace UserService.Application.Addresses.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;

public class GetPrimaryAddressQuery : IRequest<Result<AddressDto?>>
{
    public Guid UserId { get; set; }
}

