using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Shared;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Добавляет JWT аутентификацию для микросервисов, использующих токены от AuthService
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Приоритет: переменная окружения > конфигурация
        var secretKey = Environment.GetEnvironmentVariable("Authentication__Jwt__SecretKey")
            ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
            ?? configuration["Authentication:Jwt:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is not configured. " +
                "Set it in appsettings.json or environment variable JWT_SECRET_KEY or Authentication__Jwt__SecretKey");
        
        var issuer = Environment.GetEnvironmentVariable("Authentication__Jwt__Issuer")
            ?? configuration["Authentication:Jwt:Issuer"] 
            ?? "AuthService";
        
        var audience = Environment.GetEnvironmentVariable("Authentication__Jwt__Audience")
            ?? configuration["Authentication:Jwt:Audience"] 
            ?? "AuthServiceClients";

        var key = Encoding.UTF8.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}

