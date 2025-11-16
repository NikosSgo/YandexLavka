using AuthService.Application.Dto;
using AuthService.Domain.Entities;

namespace AuthService.Application.Mapping;

public static class AccountMapping
{
    public static AccountDto ToDto(this Account account)
    {
        return new AccountDto
        {
            Id = account.Id,
            Email = account.Email.Value,
            IsActive = account.IsActive,
            LastLoginAt = account.LastLoginAt,
            CreatedAt = account.CreatedAt
        };
    }
}


