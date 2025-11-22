// WareHouse.Domain/Entities/Product.cs
using WareHouse.Domain.Common;

namespace WareHouse.Domain.Entities;

public class Product : Entity
{
    public string Name { get; set; }
    public string Sku { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? WeightKg { get; set; }
    public bool RequiresRefrigeration { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Конструктор для Dapper
    public Product()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // Конструктор для создания нового продукта с ID
    public Product(Guid id, string name, string sku, string description, string category, 
        decimal unitPrice, decimal? weightKg, bool requiresRefrigeration) : base(id)
    {
        Name = name;
        Sku = sku;
        Description = description;
        Category = category;
        UnitPrice = unitPrice;
        WeightKg = weightKg;
        RequiresRefrigeration = requiresRefrigeration;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}