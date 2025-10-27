namespace WareHouse.Application.DTOs;

public record StockLevelDto
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; }
    public string Sku { get; init; }
    public int TotalQuantity { get; init; }
    public int AvailableQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public bool IsLowStock { get; init; }
    public bool IsOutOfStock { get; init; }
    public List<StorageUnitDto> StorageUnits { get; init; } = new();
}

public record StorageUnitDto
{
    public Guid UnitId { get; init; }
    public string Location { get; init; }
    public string Zone { get; init; }
    public int Quantity { get; init; }
    public int AvailableQuantity { get; init; }
    public int ReservedQuantity { get; init; }
    public DateTime LastRestocked { get; init; }
}

public record StockUpdateRequest
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public string Location { get; init; }
    public string Operation { get; init; } // "restock", "adjust"
    public string Reason { get; init; }
}