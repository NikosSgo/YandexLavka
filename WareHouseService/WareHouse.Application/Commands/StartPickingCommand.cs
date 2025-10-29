using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record StartPickingCommand(Guid OrderId, string PickerId, string Zone) : IRequest<PickingTaskDto>;

public class StartPickingCommandValidator : AbstractValidator<StartPickingCommand>
{
    public StartPickingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");
        RuleFor(x => x.PickerId).NotEmpty().WithMessage("PickerId is required");
        RuleFor(x => x.Zone).NotEmpty().WithMessage("Zone is required");
    }
}

public class StartPickingCommandHandler : IRequestHandler<StartPickingCommand, PickingTaskDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<StartPickingCommandHandler> _logger;

    public StartPickingCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<StartPickingCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PickingTaskDto> Handle(StartPickingCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _logger.LogInformation("Starting picking for order {OrderId} by picker {PickerId}",
                request.OrderId, request.PickerId);

            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new NotFoundException($"Order {request.OrderId} not found");

            if (order.Status != OrderStatus.Received)
                throw new DomainException($"Cannot start picking for order in {order.Status} status");

            // Проверяем существующее активное задание
            var existingTask = await _unitOfWork.PickingTasks.GetActiveForOrderAsync(request.OrderId);
            if (existingTask != null)
                throw new DomainException($"Active picking task already exists for order {request.OrderId}");

            // Получаем оптимальные локации для сборки
            var storageUnits = await _unitOfWork.StorageUnits.GetUnitsForOrderAsync(request.OrderId);
            var pickingItems = CreatePickingItems(order, storageUnits);

            // Создаем задание на сборку
            var pickingTask = new PickingTask(request.OrderId, pickingItems, request.Zone, request.PickerId);
            pickingTask.StartPicking(request.PickerId);

            // Обновляем статус заказа
            order.StartPicking();

            await _unitOfWork.PickingTasks.AddAsync(pickingTask);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Picking task {TaskId} created for order {OrderId}",
                pickingTask.TaskId, request.OrderId);

            return PickingTaskDto.FromEntity(pickingTask);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private List<PickingItem> CreatePickingItems(OrderAggregate order, List<StorageUnit> storageUnits)
    {
        var pickingItems = new List<PickingItem>();

        foreach (var line in order.Lines)
        {
            var availableUnits = storageUnits
                .Where(u => u.ProductId == line.ProductId && u.AvailableQuantity > 0)
                .OrderByDescending(u => u.AvailableQuantity)
                .ToList();

            var remainingQty = line.QuantityOrdered;
            foreach (var unit in availableUnits)
            {
                var qtyToPick = Math.Min(remainingQty, unit.AvailableQuantity);

                var pickingItem = new PickingItem(
                    Guid.Empty, // TaskId будет установлен в конструкторе PickingTask
                    line.ProductId,
                    line.ProductName,
                    line.Sku,
                    qtyToPick,
                    unit.Location,
                    $"BC-{line.ProductId}"
                );

                pickingItems.Add(pickingItem);
                remainingQty -= qtyToPick;
                if (remainingQty <= 0) break;
            }

            if (remainingQty > 0)
                throw new DomainException($"Insufficient stock for product {line.ProductName}");
        }

        return pickingItems;
    }
}