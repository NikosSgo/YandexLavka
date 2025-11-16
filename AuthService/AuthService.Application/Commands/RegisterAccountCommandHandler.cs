using AuthService.Application.Common;
using AuthService.Application.Dto;
using AuthService.Application.Mapping;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands;

public class RegisterAccountCommandHandler : IRequestHandler<RegisterAccountCommand, Result<TokenDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterAccountCommandHandler> _logger;

    public RegisterAccountCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService,
        ILogger<RegisterAccountCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<TokenDto>> Handle(RegisterAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Проверка на существование аккаунта с таким email
            var exists = await _accountRepository.ExistsByEmailAsync(request.Email);
            if (exists)
            {
                return Result<TokenDto>.Failure("Account with this email already exists");
            }

            // Создание value objects
            var email = new Email(request.Email);
            var passwordHash = _passwordHashService.HashPassword(request.Password);
            var passwordHashValueObject = new PasswordHash(passwordHash);

            // Создание аккаунта
            var account = new Account(email, passwordHashValueObject);

            // Сохранение в репозиторий
            await _accountRepository.AddAsync(account);

            // Генерация токенов
            var accessToken = _jwtTokenService.GenerateToken(account);
            var refreshToken = _jwtTokenService.GenerateRefreshToken();

            var tokenDto = new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Account = account.ToDto()
            };

            _logger.LogInformation("Account registered successfully: {Email}", request.Email);

            return Result<TokenDto>.Success(tokenDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register account: {Email}", request.Email);
            return Result<TokenDto>.Failure($"Failed to register account: {ex.Message}", ex);
        }
    }
}


