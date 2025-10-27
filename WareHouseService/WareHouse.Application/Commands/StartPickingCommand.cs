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
    private readonly IOrderRepository _orderRepository;
    private readonly IPickingTaskRepository _pickingTaskRepository;
    private readonly IStorageUnitRepository _storageUnitRepository;
    private readonly ILogger<StartPickingCommandHandler> _logger;

    public StartPickingCommandHandler(
        IOrderRepository orderRepository,
        IPickingTaskRepository pickingTaskRepository,
        IStorageUnitRepository storageUnitRepository,
        ILogger<StartPickingCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _pickingTaskRepository = pickingTaskRepository;
        _storageUnitRepository = storageUnitRepository;
        _logger = logger;
    }

    public async Task<PickingTaskDto> Handle(StartPickingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting picking for order {OrderId} by picker {PickerId}",
            request.OrderId, request.PickerId);

        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
            throw new NotFoundException($"Order {request.OrderId} not found");

        if (order.Status != OrderStatus.Received)
            throw new DomainException($"Cannot start picking for order in {order.Status} status");

        // Проверяем существующее активное задание
        var existingTask = await _pickingTaskRepository.GetActiveForOrderAsync(request.OrderId);
        if (existingTask != null)
            throw new DomainException($"Active picking task already exists for order {request.OrderId}");

        // Получаем оптимальные локации для сборки
        var storageUnits = await _storageUnitRepository.GetUnitsForOrderAsync(request.OrderId);
        var pickingItems = CreatePickingItems(order, storageUnits);

        // Создаем задание на сборку
        var pickingTask = new PickingTask(request.OrderId, pickingItems, request.Zone, request.PickerId);
        pickingTask.StartPicking(request.PickerId);

        // Обновляем статус заказа
        order.StartPicking();

        await _pickingTaskRepository.AddAsync(pickingTask);
        await _orderRepository.UpdateAsync(order);
        await _pickingTaskRepository.SaveChangesAsync();

        _logger.LogInformation("Picking task {TaskId} created for order {OrderId}",
            pickingTask.TaskId, request.OrderId);

        return PickingTaskDto.FromEntity(pickingTask);
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
                pickingItems.Add(new PickingItem(
                    line.ProductId,
                    line.ProductName,
                    line.Sku,
                    qtyToPick,
                    unit.Location,
                    $"BC-{line.ProductId}" // Генерируем штрих-код
                ));

                remainingQty -= qtyToPick;
                if (remainingQty <= 0) break;
            }

            if (remainingQty > 0)
                throw new DomainException($"Insufficient stock for product {line.ProductName}");
        }

        return pickingItems;
    }
}