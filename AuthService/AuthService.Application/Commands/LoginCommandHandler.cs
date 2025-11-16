using AuthService.Application.Common;
using AuthService.Application.Dto;
using AuthService.Application.Mapping;
using AuthService.Application.Services;
using AuthService.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IAccountRepository accountRepository,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _accountRepository = accountRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<Result<TokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Поиск аккаунта по email
            var account = await _accountRepository.GetByEmailAsync(request.Email);
            if (account == null)
            {
                return Result<TokenDto>.Failure("Invalid email or password");
            }

            // Проверка активности аккаунта
            if (!account.IsActive)
            {
                return Result<TokenDto>.Failure("Account is deactivated");
            }

            // Проверка пароля
            var isPasswordValid = _passwordHashService.VerifyPassword(request.Password, account.PasswordHash.Value);
            if (!isPasswordValid)
            {
                return Result<TokenDto>.Failure("Invalid email or password");
            }

            // Запись времени последнего входа
            account.RecordLogin();
            await _accountRepository.UpdateAsync(account);

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

            _logger.LogInformation("Account logged in successfully: {Email}", request.Email);

            return Result<TokenDto>.Success(tokenDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to login account: {Email}", request.Email);
            return Result<TokenDto>.Failure($"Failed to login: {ex.Message}", ex);
        }
    }
}


