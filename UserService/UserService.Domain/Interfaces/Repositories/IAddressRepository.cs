namespace UserService.Domain.Interfaces.Repositories;

using UserService.Domain.Entities;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id);
    Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId);
    Task<Address?> GetPrimaryAddressAsync(Guid userId);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(Address address);
}
