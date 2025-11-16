using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces.Repositories;
using AuthService.Domain.ValueObjects;
using AuthService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        
        try
        {
            var emailValueObject = new Email(normalizedEmail);
            
            return await _context.Accounts
                .FirstOrDefaultAsync(a => a.Email == emailValueObject);
        }
        catch
        {
            return null;
        }
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Account account)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        try
        {
            var emailValueObject = new Email(normalizedEmail);
            return await _context.Accounts
                .AnyAsync(a => a.Email == emailValueObject);
        }
        catch
        {
            return false;
        }
    }
}


