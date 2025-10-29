using MediatR;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetStockLevelQuery(Guid ProductId) : IRequest<StockLevelDto>;

public class GetStockLevelQueryHandler : IRequestHandler<GetStockLevelQuery, StockLevelDto>
{
    private readonly IStorageUnitRepository _storageUnitRepository;

    public GetStockLevelQueryHandler(IStorageUnitRepository storageUnitRepository)
    {
        _storageUnitRepository = storageUnitRepository;
    }

    public async Task<StockLevelDto> Handle(GetStockLevelQuery request, CancellationToken cancellationToken)
    {
        var storageUnits = await _storageUnitRepository.GetByProductAsync(request.ProductId);

        if (!storageUnits.Any())
            throw new NotFoundException($"No stock found for product {request.ProductId}");

        var firstUnit = storageUnits.First();

        return new StockLevelDto
        {
            ProductId = request.ProductId,
            ProductName = firstUnit.ProductName,
            Sku = firstUnit.Sku,
            TotalQuantity = storageUnits.Sum(u => u.Quantity),
            AvailableQuantity = storageUnits.Sum(u => u.AvailableQuantity),
            ReservedQuantity = storageUnits.Sum(u => u.ReservedQuantity),
            IsLowStock = storageUnits.Any(u => u.IsLowStock),
            IsOutOfStock = storageUnits.All(u => u.IsOutOfStock),
            StorageUnits = storageUnits.Select(u => new StorageUnitDto
            {
                UnitId = u.Id,
                Location = u.Location,
                Zone = u.Zone,
                Quantity = u.Quantity,
                AvailableQuantity = u.AvailableQuantity,
                ReservedQuantity = u.ReservedQuantity,
                LastRestocked = u.LastRestocked
            }).ToList()
        };
    }
}