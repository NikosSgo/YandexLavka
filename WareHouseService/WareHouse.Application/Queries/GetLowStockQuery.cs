using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetLowStockQuery : IRequest<List<StockLevelDto>>;

public class GetLowStockQueryHandler : IRequestHandler<GetLowStockQuery, List<StockLevelDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetLowStockQueryHandler> _logger;

    public GetLowStockQueryHandler(IUnitOfWork unitOfWork, ILogger<GetLowStockQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<StockLevelDto>> Handle(GetLowStockQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting low stock items");

        var lowStockUnits = await _unitOfWork.StorageUnits.GetLowStockUnitsAsync();

        return lowStockUnits.Select(unit => new StockLevelDto
        {
            ProductId = unit.ProductId,
            ProductName = unit.ProductName,
            Sku = unit.Sku,
            Quantity = unit.Quantity,
            ReservedQuantity = unit.ReservedQuantity,
            AvailableQuantity = unit.AvailableQuantity,
            Location = unit.Location,
            Zone = unit.Zone,
            LastRestocked = unit.LastRestocked,
            IsLowStock = unit.IsLowStock,
            IsOutOfStock = unit.IsOutOfStock
        }).ToList();
    }
}