namespace UserService.Domain.Interfaces.Repositories;

using UserService.Domain.Entities;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByPhoneAsync(string phone);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);

    Task<bool> ExistsByPhoneAsync(string phone);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User?> GetUserWithAddressesAsync(Guid userId);
}
