using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetStockLevelQuery(Guid ProductId) : IRequest<StockLevelDto>;

public class GetStockLevelQueryHandler : IRequestHandler<GetStockLevelQuery, StockLevelDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetStockLevelQueryHandler> _logger;

    public GetStockLevelQueryHandler(IUnitOfWork unitOfWork, ILogger<GetStockLevelQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<StockLevelDto> Handle(GetStockLevelQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stock level for product {ProductId}", request.ProductId);

        var storageUnits = await _unitOfWork.StorageUnits.GetByProductAsync(request.ProductId);

        if (!storageUnits.Any())
            throw new NotFoundException($"Product {request.ProductId} not found");

        var totalQuantity = storageUnits.Sum(u => u.Quantity);
        var totalReserved = storageUnits.Sum(u => u.ReservedQuantity);
        var mainUnit = storageUnits.First();

        return new StockLevelDto
        {
            ProductId = request.ProductId,
            ProductName = mainUnit.ProductName,
            Sku = mainUnit.Sku,
            Quantity = totalQuantity,
            ReservedQuantity = totalReserved,
            AvailableQuantity = totalQuantity - totalReserved,
            Location = storageUnits.Count > 1 ? "Multiple" : mainUnit.Location,
            Zone = mainUnit.Zone,
            LastRestocked = storageUnits.Max(u => u.LastRestocked),
            IsLowStock = totalQuantity - totalReserved <= 10,
            IsOutOfStock = totalQuantity - totalReserved == 0,
            StorageLocations = storageUnits.Select(u => new StorageLocationDto
            {
                Location = u.Location,
                Quantity = u.Quantity,
                AvailableQuantity = u.AvailableQuantity
            }).ToList()
        };
    }
}