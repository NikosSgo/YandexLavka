namespace UserService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces.Repositories;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByPhoneAsync(string phone)
    {
        // Нормализуем телефон для поиска
        var normalizedPhone = NormalizePhoneForSearch(phone);
        
        // EF Core автоматически конвертирует Phone value object в строку в БД
        // Используем сравнение через создание Phone объекта - EF Core преобразует это в SQL благодаря конвертации
        try
        {
            var phoneValueObject = new Phone(normalizedPhone);
            
            // Используем сравнение value objects - EF Core использует конвертацию для преобразования в SQL
            return await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Phone == phoneValueObject);
        }
        catch
        {
            // Если телефон невалидный, возвращаем null
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Нормализуем email для поиска
        var normalizedEmail = email.ToLower().Trim();
        
        // Используем сравнение через создание Email объекта - EF Core преобразует это в SQL благодаря конвертации
        try
        {
            var emailValueObject = new Email(normalizedEmail);
            
            return await _context.Users
                .Include(u => u.Addresses)
                .FirstOrDefaultAsync(u => u.Email == emailValueObject);
        }
        catch
        {
            // Если email невалидный, возвращаем null
            return null;
        }
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Addresses)
            .ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsByPhoneAsync(string phone)
    {
        var normalizedPhone = NormalizePhoneForSearch(phone);
        try
        {
            var phoneValueObject = new Phone(normalizedPhone);
            // Используем сравнение value objects - EF Core использует конвертацию для преобразования в SQL
            return await _context.Users
                .AnyAsync(u => u.Phone == phoneValueObject);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        var normalizedEmail = email.ToLower().Trim();
        try
        {
            var emailValueObject = new Email(normalizedEmail);
            // Используем сравнение value objects - EF Core использует конвертацию для преобразования в SQL
            return await _context.Users
                .AnyAsync(u => u.Email == emailValueObject);
        }
        catch
        {
            return false;
        }
    }

    public async Task<User?> GetUserWithAddressesAsync(Guid userId)
    {
        return await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    private static string NormalizePhoneForSearch(string phone)
    {
        // Используем ту же логику нормализации, что и в Phone value object
        var digits = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");

        if (digits.StartsWith("8") && digits.Length == 11)
        {
            return "+7" + digits.Substring(1);
        }

        if (digits.Length == 10)
        {
            return "+7" + digits;
        }

        if (phone.StartsWith("+"))
        {
            return phone;
        }

        return "+" + digits;
    }
}

