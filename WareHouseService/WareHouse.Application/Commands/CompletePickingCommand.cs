using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record CompletePickingCommand(Guid OrderId, Dictionary<Guid, int> PickedQuantities) : IRequest<Unit>;

public class CompletePickingCommandValidator : AbstractValidator<CompletePickingCommand>
{
    public CompletePickingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");
        RuleFor(x => x.PickedQuantities).NotEmpty().WithMessage("Picked quantities are required");
        RuleForEach(x => x.PickedQuantities)
            .Must(kvp => kvp.Value > 0)
            .WithMessage("Quantity must be positive");
    }
}

public class CompletePickingCommandHandler : IRequestHandler<CompletePickingCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CompletePickingCommandHandler> _logger;

    public CompletePickingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CompletePickingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(CompletePickingCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Completing picking for order {OrderId}", request.OrderId);

            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new NotFoundException($"Order {request.OrderId} not found");

            var pickingTask = await _unitOfWork.PickingTasks.GetActiveForOrderAsync(request.OrderId);
            if (pickingTask == null)
                throw new NotFoundException($"Active picking task not found for order {request.OrderId}");

            // Резервируем товары на складе
            await ReserveStockForOrder(request.PickedQuantities);

            // Завершаем сборку
            order.CompletePicking(request.PickedQuantities);
            pickingTask.Complete();

            // Обновляем статус собранных товаров в задании
            foreach (var (productId, quantity) in request.PickedQuantities)
            {
                pickingTask.UpdateItemPickedStatus(productId, quantity);
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.PickingTasks.UpdateAsync(pickingTask);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Picking completed for order {OrderId}", request.OrderId);

            return Unit.Value;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task ReserveStockForOrder(Dictionary<Guid, int> pickedQuantities)
    {
        foreach (var (productId, quantity) in pickedQuantities)
        {
            var storageUnits = await _unitOfWork.StorageUnits.GetByProductAsync(productId);
            var remainingQty = quantity;

            foreach (var unit in storageUnits.Where(u => u.AvailableQuantity > 0))
            {
                var qtyToReserve = Math.Min(remainingQty, unit.AvailableQuantity);
                unit.Reserve(qtyToReserve);
                await _unitOfWork.StorageUnits.UpdateAsync(unit);

                remainingQty -= qtyToReserve;
                if (remainingQty <= 0) break;
            }

            if (remainingQty > 0)
                throw new DomainException($"Cannot reserve {quantity} units for product {productId}");
        }
    }
}