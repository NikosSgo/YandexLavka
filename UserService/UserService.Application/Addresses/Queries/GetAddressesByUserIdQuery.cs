namespace UserService.Application.Addresses.Queries;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;

public class GetAddressesByUserIdQuery : IRequest<Result<List<AddressDto>>>
{
    public Guid UserId { get; set; }
}

