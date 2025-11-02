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

    // ✅ ДОБАВЬТЕ КОНСТРУКТОР ЕСЛИ НУЖНО
    public Product()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}