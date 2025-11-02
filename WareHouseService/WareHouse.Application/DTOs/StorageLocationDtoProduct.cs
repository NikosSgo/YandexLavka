// WareHouse.Application/DTOs/StorageLocationDtoProduct.cs - специфично для продуктов
namespace WareHouse.Application.DTOs;

public class StorageLocationDtoProduct
{
    public string Location { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Zone { get; set; } = string.Empty; // ✅ Дополнительное поле для продуктов
    public DateTime LastRestocked { get; set; } // ✅ Дополнительное поле для продуктов
}