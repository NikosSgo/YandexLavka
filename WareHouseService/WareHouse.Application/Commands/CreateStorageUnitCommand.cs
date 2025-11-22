using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record CreateStorageUnitCommand(
    Guid ProductId,
    string Location,
    int Quantity,
    string? Zone = null
) : IRequest<StorageUnitDto>;

public class CreateStorageUnitCommandValidator : AbstractValidator<CreateStorageUnitCommand>
{
    public CreateStorageUnitCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty().WithMessage("Product ID is required");
        RuleFor(x => x.Location)
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Location is required and must not exceed 20 characters");
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero");
        RuleFor(x => x.Zone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Zone))
            .WithMessage("Zone must not exceed 20 characters");
    }
}

public class CreateStorageUnitCommandHandler : IRequestHandler<CreateStorageUnitCommand, StorageUnitDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateStorageUnitCommandHandler> _logger;

    public CreateStorageUnitCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateStorageUnitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<StorageUnitDto> Handle(CreateStorageUnitCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Creating storage unit for product {ProductId} at location {Location} with quantity {Quantity}",
                request.ProductId, request.Location, request.Quantity);

            // Проверяем существование продукта
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {request.ProductId} not found");
            }

            // Проверяем, не существует ли уже единица хранения для этого продукта в этом месте
            var existingUnit = await _unitOfWork.StorageUnits.GetByProductAndLocationAsync(
                request.ProductId, request.Location);
            
            if (existingUnit != null)
            {
                throw new InvalidOperationException(
                    $"Storage unit already exists for product {request.ProductId} at location {request.Location}. " +
                    "Use restock endpoint to add more items.");
            }

            // Определяем зону из location, если не указана
            var zone = request.Zone ?? GetZoneFromLocation(request.Location);

            // Создаем новую единицу хранения
            var storageUnit = new StorageUnit(
                productId: request.ProductId,
                productName: product.Name,
                sku: product.Sku,
                quantity: request.Quantity,
                location: request.Location,
                zone: zone
            );

            await _unitOfWork.StorageUnits.AddAsync(storageUnit);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Storage unit created successfully with ID {StorageUnitId} for product {ProductId} at {Location}",
                storageUnit.Id, request.ProductId, request.Location);

            return MapToDto(storageUnit, product);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error creating storage unit for product {ProductId} at location {Location}",
                request.ProductId, request.Location);
            throw;
        }
    }

    private string GetZoneFromLocation(string location)
    {
        if (string.IsNullOrEmpty(location))
            return "DEFAULT";

        // Если location начинается с буквы, используем её как зону
        if (location.Length > 0 && char.IsLetter(location[0]))
        {
            return location.Substring(0, 1).ToUpper();
        }

        // Если location содержит дефис, берем часть до дефиса
        var parts = location.Split('-');
        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
        {
            return parts[0].ToUpper();
        }

        return "DEFAULT";
    }

    private StorageUnitDto MapToDto(StorageUnit storageUnit, Domain.Entities.Product product)
    {
        return new StorageUnitDto
        {
            Id = storageUnit.Id,
            ProductId = storageUnit.ProductId,
            ProductName = product.Name,
            Sku = product.Sku,
            Quantity = storageUnit.Quantity,
            ReservedQuantity = storageUnit.ReservedQuantity,
            AvailableQuantity = storageUnit.AvailableQuantity,
            Location = storageUnit.Location,
            Zone = storageUnit.Zone,
            LastRestocked = storageUnit.LastRestocked,
            IsLowStock = storageUnit.IsLowStock,
            IsOutOfStock = storageUnit.IsOutOfStock,
            CreatedAt = storageUnit.CreatedAt,
            UpdatedAt = storageUnit.UpdatedAt ?? storageUnit.CreatedAt
        };
    }
}

