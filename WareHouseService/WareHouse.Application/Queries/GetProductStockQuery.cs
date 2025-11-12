// GetProductStockQueryHandler - использует StorageLocationDtoProduct
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetProductStockQuery(Guid ProductId) : IRequest<ProductStockDto>;

public class GetProductStockQueryHandler : IRequestHandler<GetProductStockQuery, ProductStockDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetProductStockQueryHandler> _logger;

    public GetProductStockQueryHandler(IUnitOfWork unitOfWork, ILogger<GetProductStockQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ProductStockDto> Handle(GetProductStockQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stock level for product {ProductId}", request.ProductId);

        // Сначала проверяем существование продукта
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            _logger.LogWarning("Product {ProductId} not found", request.ProductId);
            throw new NotFoundException($"Product {request.ProductId} not found");
        }

        // Получаем все storage units для продукта
        var storageUnits = await _unitOfWork.StorageUnits.GetByProductAsync(request.ProductId);
        storageUnits ??= new List<Domain.Entities.StorageUnit>();

        _logger.LogInformation("Found {Count} storage units for product {ProductId}",
            storageUnits.Count, request.ProductId);

        var totalQuantity = storageUnits.Sum(u => u.Quantity);
        var totalReserved = storageUnits.Sum(u => u.ReservedQuantity);
        var availableQuantity = totalQuantity - totalReserved;

        return new ProductStockDto
        {
            ProductId = request.ProductId,
            ProductName = product.Name,
            Sku = product.Sku,
            TotalQuantity = totalQuantity,
            ReservedQuantity = totalReserved,
            AvailableQuantity = availableQuantity,
            Locations = storageUnits.Select(u => u.Location).ToList(),
            IsLowStock = availableQuantity <= 10,
            IsOutOfStock = availableQuantity == 0,
            StorageLocations = storageUnits.Select(u => new StorageLocationDtoProduct
            {
                Location = u.Location,
                Quantity = u.Quantity,
                AvailableQuantity = u.Quantity - u.ReservedQuantity,
                Zone = u.Zone,
                LastRestocked = u.LastRestocked
            }).ToList()
        };
    }
}