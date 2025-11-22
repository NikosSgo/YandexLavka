using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Interfaces;

public interface ICartRepository
{
    Task<CartItem?> GetByIdAsync(Guid id);
    Task<CartItem?> GetByCustomerAndProductAsync(string customerId, Guid productId);
    Task<List<CartItem>> GetByCustomerIdAsync(string customerId);
    Task<CartItem> AddAsync(CartItem entity);
    Task UpdateAsync(CartItem entity);
    Task DeleteAsync(CartItem entity);
    Task DeleteByCustomerIdAsync(string customerId);
    Task<bool> ExistsAsync(string customerId, Guid productId);
    Task<int> GetItemCountAsync(string customerId);
}

