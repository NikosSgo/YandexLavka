using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
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

            // 1. Получаем активное задание на сборку
            var pickingTask = await _unitOfWork.PickingTasks.GetActiveForOrderAsync(request.OrderId);
            if (pickingTask == null)
                throw new NotFoundException($"Active picking task not found for order {request.OrderId}");

            // 2. ✅ ОБНОВЛЯЕМ quantity_picked в order_lines
            await UpdateOrderLinesPickedQuantities(request.OrderId, request.PickedQuantities);

            // 3. Обновляем статус picked в picking items
            foreach (var (productId, quantityPicked) in request.PickedQuantities)
            {
                pickingTask.UpdateItemPickedStatus(productId, quantityPicked);
            }

            // 4. Завершаем задание
            pickingTask.Complete();

            // 5. Обновляем статус заказа
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            order.CompletePicking(request.PickedQuantities); // ✅ Передаем pickedQuantities

            // 6. Резервируем stock
            await ReserveStockForOrder(request.PickedQuantities);

            // 7. Сохраняем изменения
            await _unitOfWork.PickingTasks.UpdateAsync(pickingTask);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Picking completed for order {OrderId}", request.OrderId);

            return Unit.Value; // ✅ Возвращаем Unit.Value для MediatR
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private async Task UpdateOrderLinesPickedQuantities(Guid orderId, Dictionary<Guid, int> pickedQuantities)
    {
        // ✅ Используем OrderRepository для обновления order_lines
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        foreach (var (productId, quantityPicked) in pickedQuantities)
        {
            var orderLine = order.Lines.FirstOrDefault(line => line.ProductId == productId);
            if (orderLine != null)
            {
                // Обновляем quantity_picked в order line
                orderLine.GetType().GetProperty("QuantityPicked")?
                    .SetValue(orderLine, quantityPicked);
            }
        }

        await _unitOfWork.Orders.UpdateAsync(order);
        _logger.LogInformation("Updated order_lines picked quantities for order {OrderId}", orderId);
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