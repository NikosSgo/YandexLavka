using AuthService.Application.Common;
using AuthService.Application.Dto;
using AuthService.Application.Mapping;
using AuthService.Domain.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthService.Application.Queries;

public class GetAccountQueryHandler : IRequestHandler<GetAccountQuery, Result<AccountDto>>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger<GetAccountQueryHandler> _logger;

    public GetAccountQueryHandler(IAccountRepository accountRepository, ILogger<GetAccountQueryHandler> logger)
    {
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task<Result<AccountDto>> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var account = await _accountRepository.GetByIdAsync(request.AccountId);
            if (account == null)
            {
                return Result<AccountDto>.Failure("Account not found");
            }

            return Result<AccountDto>.Success(account.ToDto());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get account: {AccountId}", request.AccountId);
            return Result<AccountDto>.Failure($"Failed to get account: {ex.Message}", ex);
        }
    }
}


