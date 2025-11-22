using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record DeleteProductCommand(Guid ProductId) : IRequest<Unit>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteProductCommandHandler> _logger;

    public DeleteProductCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteProductCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Deleting product {ProductId}", request.ProductId);

            // Проверяем существование продукта
            var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {request.ProductId} not found");
            }

            // Проверяем, есть ли товар на складе (storage_units)
            var storageUnits = await _unitOfWork.StorageUnits.GetByProductAsync(request.ProductId);
            if (storageUnits != null && storageUnits.Any())
            {
                // Проверяем, есть ли зарезервированные товары
                var hasReservedItems = storageUnits.Any(su => su.ReservedQuantity > 0);
                if (hasReservedItems)
                {
                    throw new InvalidOperationException(
                        $"Cannot delete product {request.ProductId}: there are reserved items in stock. " +
                        "Please wait until all reservations are released or cancelled.");
                }

                // Проверяем, есть ли товары на складе
                var hasStock = storageUnits.Any(su => su.Quantity > 0);
                if (hasStock)
                {
                    throw new InvalidOperationException(
                        $"Cannot delete product {request.ProductId}: there are items in stock. " +
                        "Please remove all stock items before deleting the product.");
                }
            }

            // Проверяем, используется ли продукт в активных заказах
            // (это можно расширить, если нужно проверять заказы)

            // Удаляем продукт
            await _unitOfWork.Products.DeleteAsync(product);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Product {ProductId} deleted successfully", request.ProductId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting product {ProductId}", request.ProductId);
            throw;
        }
    }
}

