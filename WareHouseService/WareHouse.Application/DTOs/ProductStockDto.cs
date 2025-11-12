// WareHouse.Application/DTOs/ProductStockDto.cs - использует StorageLocationDtoProduct
namespace WareHouse.Application.DTOs;

public class ProductStockDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public List<string> Locations { get; set; } = new();
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public List<StorageLocationDtoProduct> StorageLocations { get; set; } = new(); // ✅ PRODUCT ДЛЯ ProductStockDto
}