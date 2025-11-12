namespace UserService.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Interfaces.Repositories;
using UserService.Infrastructure.Data;

public class AddressRepository : IAddressRepository
{
    private readonly ApplicationDbContext _context;

    public AddressRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Address?> GetByIdAsync(Guid id)
    {
        return await _context.Addresses
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId)
    {
        // Поскольку Address - часть агрегата User, получаем через User
        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.Addresses ?? Enumerable.Empty<Address>();
    }

    public async Task<Address?> GetPrimaryAddressAsync(Guid userId)
    {
        // Поскольку Address - часть агрегата User, получаем через User
        var user = await _context.Users
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.GetPrimaryAddress();
    }

    public async Task AddAsync(Address address)
    {
        // Address добавляется через User агрегат, поэтому этот метод не должен использоваться напрямую
        // Но для совместимости с интерфейсом добавляем реализацию
        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Address address)
    {
        // Address обновляется через User агрегат, поэтому этот метод не должен использоваться напрямую
        // Но для совместимости с интерфейсом добавляем реализацию
        _context.Addresses.Update(address);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Address address)
    {
        // Address удаляется через User агрегат, поэтому этот метод не должен использоваться напрямую
        // Но для совместимости с интерфейсом добавляем реализацию
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
    }
}

