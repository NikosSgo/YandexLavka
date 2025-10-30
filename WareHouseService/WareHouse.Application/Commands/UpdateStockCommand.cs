using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record UpdateStockCommand(Guid ProductId, int Quantity, string Location, string Operation)
    : IRequest<Unit>;

public class UpdateStockCommandValidator : AbstractValidator<UpdateStockCommand>
{
    public UpdateStockCommandValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Location).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Operation).NotEmpty().Must(BeValidOperation)
            .WithMessage("Operation must be 'restock' or 'adjust'");
    }

    private bool BeValidOperation(string operation)
    {
        return operation == "restock" || operation == "adjust";
    }
}

public class UpdateStockCommandHandler : IRequestHandler<UpdateStockCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateStockCommandHandler> _logger;

    public UpdateStockCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateStockCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Updating stock for product {ProductId} at {Location}. Operation: {Operation}",
                request.ProductId, request.Location, request.Operation);

            var storageUnit = await _unitOfWork.StorageUnits.GetByProductAndLocationAsync(
                request.ProductId, request.Location);

            if (storageUnit == null)
            {
                // Создаем новый storage unit если не существует
                _logger.LogInformation("Creating new storage unit for product {ProductId} at {Location}",
                    request.ProductId, request.Location);

                storageUnit = CreateNewStorageUnit(request.ProductId, request.Location);
                await _unitOfWork.StorageUnits.AddAsync(storageUnit);

                _logger.LogInformation("New storage unit created with ID: {StorageUnitId}", storageUnit.Id);
            }

            // Выполняем операцию
            if (request.Operation == "restock")
            {
                storageUnit.Restock(request.Quantity);
                _logger.LogInformation("Restocked {Quantity} units for product {ProductId}",
                    request.Quantity, request.ProductId);
            }
            else if (request.Operation == "adjust")
            {
                AdjustStock(storageUnit, request.Quantity);
                _logger.LogInformation("Adjusted stock to {Quantity} units for product {ProductId}",
                    request.Quantity, request.ProductId);
            }

            await _unitOfWork.StorageUnits.UpdateAsync(storageUnit);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Stock updated successfully for product {ProductId}. New quantity: {Quantity}",
                request.ProductId, storageUnit.Quantity);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error updating stock for product {ProductId}", request.ProductId);
            throw;
        }
    }

    private StorageUnit CreateNewStorageUnit(Guid productId, string location)
    {
        var zone = GetZoneFromLocation(location);
        var productName = $"Product-{productId}";
        var sku = $"SKU-{productId}";

        return new StorageUnit(
            productId: productId,
            productName: productName,
            sku: sku,
            quantity: 0, // Начальное количество 0
            location: location,
            zone: zone
        );
    }

    private void AdjustStock(StorageUnit storageUnit, int newQuantity)
    {
        if (newQuantity < 0)
        {
            throw new DomainException("Quantity cannot be negative");
        }

        // Используем публичный метод для установки количества
        storageUnit.SetQuantity(newQuantity);
    }

    private string GetZoneFromLocation(string location)
    {
        if (!string.IsNullOrEmpty(location) && location.Length > 0 && char.IsLetter(location[0]))
        {
            return location.Substring(0, 1).ToUpper();
        }
        return "DEFAULT";
    }
}