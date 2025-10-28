using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record CancelPickingCommand(Guid OrderId, string Reason) : IRequest<Unit>;

public class CancelPickingCommandValidator : AbstractValidator<CancelPickingCommand>
{
    public CancelPickingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500).WithMessage("Reason is required");
    }
}

public class CancelPickingCommandHandler : IRequestHandler<CancelPickingCommand, Unit>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPickingTaskRepository _pickingTaskRepository;
    private readonly ILogger<CancelPickingCommandHandler> _logger;

    public CancelPickingCommandHandler(
        IOrderRepository orderRepository,
        IPickingTaskRepository pickingTaskRepository,
        ILogger<CancelPickingCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _pickingTaskRepository = pickingTaskRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelPickingCommand request, CancellationToken cancellationToken)
    {

        // ОСВОБОЖДЕНИЕ резерваций при отмене
        //var reservations = await _reservationRepository.GetByOrderAsync(orderId);
        //foreach (var reservation in reservations)
        //{
        //    var storageUnit = await _storageUnitRepository.GetByIdAsync(reservation.UnitId);
        //    storageUnit.ReleaseReservation(reservation.Quantity); // ← ВЫЗОВ МЕТОДА ОСВОБОЖДЕНИЯ
        //    await _storageUnitRepository.UpdateAsync(storageUnit);
        //}

        _logger.LogInformation("Cancelling picking for order {OrderId}. Reason: {Reason}",
            request.OrderId, request.Reason);

        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new NotFoundException($"Order {request.OrderId} not found");

        var pickingTask = await _pickingTaskRepository.GetActiveForOrderAsync(request.OrderId);

        // Отменяем заказ
        order.Cancel(request.Reason);

        // Отменяем задание на сборку если есть
        if (pickingTask != null)
        {
            pickingTask.Cancel();
            await _pickingTaskRepository.UpdateAsync(pickingTask);
        }

        await _orderRepository.UpdateAsync(order);
        await _orderRepository.SaveChangesAsync();

        _logger.LogInformation("Picking cancelled for order {OrderId}", request.OrderId);

        return Unit.Value;
    }
}