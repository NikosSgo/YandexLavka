using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(Account account);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
    Guid? GetAccountIdFromToken(string token);
}


