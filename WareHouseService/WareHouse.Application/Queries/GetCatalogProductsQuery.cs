using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetCatalogProductsQuery : IRequest<List<StockLevelDto>>;

public class GetCatalogProductsQueryHandler : IRequestHandler<GetCatalogProductsQuery, List<StockLevelDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetCatalogProductsQueryHandler> _logger;

    public GetCatalogProductsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetCatalogProductsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<StockLevelDto>> Handle(GetCatalogProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Loading full warehouse catalog snapshot");

        var storageUnits = await _unitOfWork.StorageUnits.GetAllAsync();
        if (storageUnits == null || storageUnits.Count == 0)
        {
            _logger.LogWarning("No storage units returned from repository");
            return new List<StockLevelDto>();
        }

        var grouped = storageUnits.GroupBy(unit => unit.ProductId);
        var snapshot = new List<StockLevelDto>();

        foreach (var group in grouped)
        {
            var units = group.ToList();
            var firstUnit = units.First();
            var totalQuantity = units.Sum(u => u.Quantity);
            var totalReserved = units.Sum(u => u.ReservedQuantity);
            var availableQuantity = totalQuantity - totalReserved;

            snapshot.Add(new StockLevelDto
            {
                ProductId = group.Key,
                ProductName = firstUnit.ProductName,
                Sku = firstUnit.Sku,
                Quantity = totalQuantity,
                ReservedQuantity = totalReserved,
                AvailableQuantity = availableQuantity,
                Location = units.Count > 1 ? "Multiple" : firstUnit.Location,
                Zone = units.Count > 1 ? "Multiple" : firstUnit.Zone,
                LastRestocked = units.Max(u => u.LastRestocked),
                IsLowStock = availableQuantity <= 10,
                IsOutOfStock = availableQuantity == 0,
                StorageLocations = units.Select(u => new StorageLocationDto
                {
                    Location = u.Location,
                    Quantity = u.Quantity,
                    AvailableQuantity = u.AvailableQuantity
                }).ToList()
            });
        }

        return snapshot
            .OrderBy(p => p.IsOutOfStock ? 2 : p.IsLowStock ? 1 : 0)
            .ThenByDescending(p => p.AvailableQuantity)
            .ThenBy(p => p.ProductName)
            .ToList();
    }
}

