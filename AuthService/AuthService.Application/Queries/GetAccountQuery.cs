using AuthService.Application.Common;
using AuthService.Application.Dto;
using MediatR;

namespace AuthService.Application.Queries;

public class GetAccountQuery : IRequest<Result<AccountDto>>
{
    public Guid AccountId { get; set; }
}


