using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record DeleteStorageUnitCommand(Guid StorageUnitId) : IRequest<Unit>;

public class DeleteStorageUnitCommandHandler : IRequestHandler<DeleteStorageUnitCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteStorageUnitCommandHandler> _logger;

    public DeleteStorageUnitCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteStorageUnitCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteStorageUnitCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Deleting storage unit {StorageUnitId}", request.StorageUnitId);

            // Проверяем существование единицы хранения
            var storageUnit = await _unitOfWork.StorageUnits.GetByIdAsync(request.StorageUnitId);
            if (storageUnit == null)
            {
                throw new KeyNotFoundException($"Storage unit with ID {request.StorageUnitId} not found");
            }

            // Проверяем, есть ли зарезервированные товары
            if (storageUnit.ReservedQuantity > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete storage unit {request.StorageUnitId}: there are {storageUnit.ReservedQuantity} reserved items. " +
                    "Please wait until all reservations are released or cancelled.");
            }

            // Проверяем, есть ли товары на складе
            if (storageUnit.Quantity > 0)
            {
                throw new InvalidOperationException(
                    $"Cannot delete storage unit {request.StorageUnitId}: there are {storageUnit.Quantity} items in stock. " +
                    "Please remove all items before deleting the storage unit.");
            }

            // Удаляем единицу хранения
            await _unitOfWork.StorageUnits.DeleteAsync(storageUnit);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Storage unit {StorageUnitId} deleted successfully", request.StorageUnitId);

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Error deleting storage unit {StorageUnitId}", request.StorageUnitId);
            throw;
        }
    }
}

