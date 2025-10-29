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
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelPickingCommandHandler> _logger;

    public CancelPickingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CancelPickingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Unit> Handle(CancelPickingCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Cancelling picking for order {OrderId}. Reason: {Reason}",
                request.OrderId, request.Reason);

            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new NotFoundException($"Order {request.OrderId} not found");

            var pickingTask = await _unitOfWork.PickingTasks.GetActiveForOrderAsync(request.OrderId);

            // Отменяем заказ
            order.Cancel(request.Reason);

            // Отменяем задание на сборку если есть
            if (pickingTask != null)
            {
                pickingTask.Cancel();
                await _unitOfWork.PickingTasks.UpdateAsync(pickingTask);
            }

            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Picking cancelled for order {OrderId}", request.OrderId);

            return Unit.Value;
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }
}