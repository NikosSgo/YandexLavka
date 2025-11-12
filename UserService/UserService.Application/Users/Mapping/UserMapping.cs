namespace UserService.Application.Users.Mapping;

using UserService.Application.Addresses.Dto;
using UserService.Application.Addresses.Mapping;
using UserService.Application.Users.Dto;
using UserService.Domain.Entities;

public static class UserMapping
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Phone = user.Phone.Value,
            Email = user.Email.Value,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Addresses = user.Addresses.Select(a => a.ToDto(user.Id)).ToList()
        };
    }
}

