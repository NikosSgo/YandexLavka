// WareHouse.Application/Queries/GetLowStockQuery.cs
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetLowStockQuery : IRequest<List<StockLevelDto>>;

public class GetLowStockQueryHandler : IRequestHandler<GetLowStockQuery, List<StockLevelDto>>
{
    private readonly IStorageUnitRepository _storageUnitRepository; // ✅ ИСПОЛЬЗУЕМ РЕПОЗИТОРИЙ НАПРЯМУЮ
    private readonly ILogger<GetLowStockQueryHandler> _logger;

    public GetLowStockQueryHandler(
        IStorageUnitRepository storageUnitRepository, // ✅ ИЗМЕНИТЕ ПАРАМЕТР
        ILogger<GetLowStockQueryHandler> logger)
    {
        _storageUnitRepository = storageUnitRepository ?? throw new ArgumentNullException(nameof(storageUnitRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<List<StockLevelDto>> Handle(GetLowStockQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting low stock items");

        try
        {
            var lowStockUnits = await _storageUnitRepository.GetLowStockUnitsAsync();

            _logger.LogInformation("Found {Count} low stock items", lowStockUnits?.Count ?? 0);

            if (lowStockUnits == null || !lowStockUnits.Any())
            {
                _logger.LogInformation("No low stock items found");
                return new List<StockLevelDto>();
            }

            return lowStockUnits.Select(unit => new StockLevelDto
            {
                ProductId = unit.ProductId,
                ProductName = unit.ProductName ?? "Unknown Product",
                Sku = unit.Sku ?? "UNKNOWN",
                Quantity = unit.Quantity,
                ReservedQuantity = unit.ReservedQuantity,
                AvailableQuantity = unit.AvailableQuantity,
                Location = unit.Location ?? "UNKNOWN",
                Zone = unit.Zone ?? "DEFAULT",
                LastRestocked = unit.LastRestocked,
                IsLowStock = unit.IsLowStock,
                IsOutOfStock = unit.IsOutOfStock
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock items from repository");

            // Возвращаем пустой список вместо исключения
            return new List<StockLevelDto>();
        }
    }
}