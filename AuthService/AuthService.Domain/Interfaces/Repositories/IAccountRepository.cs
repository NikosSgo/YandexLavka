using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id);
    Task<Account?> GetByEmailAsync(string email);
    Task AddAsync(Account account);
    Task UpdateAsync(Account account);
    Task<bool> ExistsByEmailAsync(string email);
}


