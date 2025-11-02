using WareHouse.Domain.Entities;

namespace WareHouse.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<List<Product>> GetByCategoryAsync(string category);
    Task<List<Product>> GetAllAsync();
    Task<Product> AddAsync(Product entity);
    Task UpdateAsync(Product entity);
    Task DeleteAsync(Product entity);
}