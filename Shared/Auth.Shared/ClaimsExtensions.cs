using System.Security.Claims;

namespace Auth.Shared;

public static class ClaimsExtensions
{
    /// <summary>
    /// Извлекает account_id из ClaimsPrincipal
    /// </summary>
    public static Guid? GetAccountId(this ClaimsPrincipal user)
    {
        var accountIdClaim = user.FindFirst("account_id") 
            ?? user.FindFirst(ClaimTypes.NameIdentifier);
        
        if (accountIdClaim != null && Guid.TryParse(accountIdClaim.Value, out var accountId))
        {
            return accountId;
        }

        return null;
    }

    /// <summary>
    /// Извлекает email из ClaimsPrincipal
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal user)
    {
        return user.FindFirst("email")?.Value 
            ?? user.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Проверяет, аутентифицирован ли пользователь и имеет ли account_id
    /// </summary>
    public static bool IsAuthenticatedWithAccount(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true && user.GetAccountId().HasValue;
    }
}

