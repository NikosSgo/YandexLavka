namespace WareHouse.Application.DTOs;

public class StockLevelDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Location { get; set; } = string.Empty;
    public string Zone { get; set; } = string.Empty;
    public DateTime LastRestocked { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsOutOfStock { get; set; }
    public List<StorageLocationDto> StorageLocations { get; set; } = new();
}

public class StorageLocationDto
{
    public string Location { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
}