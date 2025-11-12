namespace UserService.Application.Addresses.Mapping;

using UserService.Application.Addresses.Dto;
using UserService.Domain.Entities;

public static class AddressMapping
{
    public static AddressDto ToDto(this Address address, Guid userId)
    {
        return new AddressDto
        {
            Id = address.Id,
            UserId = userId,
            Street = address.Street,
            City = address.City,
            State = address.State,
            Country = address.Country,
            ZipCode = address.ZipCode,
            Description = address.Description,
            IsPrimary = address.IsPrimary,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }
}

