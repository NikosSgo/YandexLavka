using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Commands;

public record StartPickingCommand(Guid OrderId, string PickerId) : IRequest<PickingTaskDto>;

public class StartPickingCommandValidator : AbstractValidator<StartPickingCommand>
{
    public StartPickingCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("OrderId is required");
        RuleFor(x => x.PickerId).NotEmpty().WithMessage("PickerId is required");
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

            // 1. Получаем заказ
            var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId);
            if (order == null)
                throw new NotFoundException($"Order {request.OrderId} not found");

            if (order.Status != OrderStatus.Received)
                throw new DomainException($"Cannot start picking for order in {order.Status} status");

            // 2. Проверяем существующее активное задание
            var existingTask = await _unitOfWork.PickingTasks.GetActiveForOrderAsync(request.OrderId);
            if (existingTask != null)
                throw new DomainException($"Active picking task already exists for order {request.OrderId}");

            // 3. Автоматически определяем зоны на основе товаров заказа
            var zones = await _unitOfWork.StorageUnits.GetZonesForOrderAsync(request.OrderId);
            if (!zones.Any())
                throw new DomainException($"No available stock found for order {request.OrderId}");

            _logger.LogInformation("Automatically determined zones for order {OrderId}: {Zones}",
                request.OrderId, string.Join(", ", zones));

            // 4. Получаем товары из всех необходимых зон
            var storageUnits = await _unitOfWork.StorageUnits.GetUnitsForOrderAsync(request.OrderId);
            var pickingItems = CreatePickingItems(order, storageUnits, zones);

            // 5. ✅ ИЗМЕНЕНО: Сохраняем ВСЕ зоны через запятую
            var allZones = string.Join(",", OptimizeZoneRoute(zones));

            // 6. Создаем задание на сборку с ВСЕМИ зонами
            var pickingTask = new PickingTask(request.OrderId, pickingItems, allZones, request.PickerId);
            pickingTask.StartPicking(request.PickerId);

            // 7. Обновляем статус заказа
            order.StartPicking();

            // 8. Сохраняем изменения
            await _unitOfWork.PickingTasks.AddAsync(pickingTask);
            await _unitOfWork.Orders.UpdateAsync(order);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Picking task {TaskId} created for order {OrderId} with zones: {Zones}",
                pickingTask.TaskId, request.OrderId, allZones);

            return PickingTaskDto.FromEntity(pickingTask);
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    private List<PickingItem> CreatePickingItems(OrderAggregate order, List<StorageUnit> storageUnits, List<string> zones)
    {
        var pickingItems = new List<PickingItem>();
        var zoneOrder = OptimizeZoneRoute(zones);

        foreach (var line in order.Lines)
        {
            var remainingQty = line.QuantityOrdered;

            // Собираем товары по зонам в оптимальном порядке
            foreach (var zone in zoneOrder)
            {
                var availableUnits = storageUnits
                    .Where(u => u.ProductId == line.ProductId &&
                               string.Equals(u.Zone, zone, StringComparison.OrdinalIgnoreCase) && // ✅ Сравнение без учета регистра
                               u.AvailableQuantity > 0)
                    .OrderByDescending(u => u.AvailableQuantity)
                    .ToList();

                foreach (var unit in availableUnits)
                {
                    var qtyToPick = Math.Min(remainingQty, unit.AvailableQuantity);

                    var pickingItem = new PickingItem(
                        Guid.Empty, // TaskId будет установлен позже
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

                if (remainingQty <= 0) break;
            }

            if (remainingQty > 0)
                throw new DomainException($"Insufficient stock for product {line.ProductName}");
        }

        return pickingItems;
    }

    private List<string> OptimizeZoneRoute(List<string> zones)
    {
        // Логика оптимизации маршрута по приоритету зон
        var zonePriority = new Dictionary<string, int>
        {
            ["A"] = 1,
            ["B"] = 2,
            ["C"] = 3,
            ["D"] = 4,
            ["cooler"] = 5,
            ["freezer"] = 6
        };

        return zones
            .Distinct()
            .OrderBy(zone => zonePriority.TryGetValue(zone, out var priority) ? priority : 999)
            .ToList();
    }

    private string DeterminePrimaryZone(List<string> zones)
    {
        // Определяем основную зону по приоритету
        var zonePriority = new Dictionary<string, int>
        {
            ["A"] = 1,
            ["B"] = 2,
            ["C"] = 3,
            ["D"] = 4,
            ["cooler"] = 5,
            ["freezer"] = 6
        };

        return zones
            .Distinct()
            .OrderBy(zone => zonePriority.TryGetValue(zone, out var priority) ? priority : 999)
            .FirstOrDefault() ?? "A";
    }
}