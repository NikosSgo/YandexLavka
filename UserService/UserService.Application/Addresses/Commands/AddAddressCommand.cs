namespace UserService.Application.Addresses.Commands;

using MediatR;
using UserService.Application.Common;
using UserService.Application.Addresses.Dto;

public class AddAddressCommand : IRequest<Result<AddressDto>>
{
    public Guid UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

