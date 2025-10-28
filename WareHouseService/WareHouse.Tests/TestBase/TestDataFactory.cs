using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;

namespace WareHouse.Tests.TestBase;

public static class TestDataFactory
{
    public static OrderAggregate CreateOrder(
        Guid? orderId = null,
        string customerId = "test-customer",
        OrderStatus status = OrderStatus.Received)
    {
        var lines = new List<OrderLine>
        {
            new OrderLine(Guid.NewGuid(), "Test Product 1", "SKU-TEST-1", 2, 100m),
            new OrderLine(Guid.NewGuid(), "Test Product 2", "SKU-TEST-2", 1, 200m)
        };

        var order = new OrderAggregate(
            orderId ?? Guid.NewGuid(),
            customerId,
            lines
        );

        // Устанавливаем статус если нужно
        if (status != OrderStatus.Received)
        {
            // Здесь можно добавить логику для установки других статусов
        }

        return order;
    }

    public static PickingTask CreatePickingTask(
        Guid? orderId = null,
        string pickerId = "test-picker",
        PickingTaskStatus status = PickingTaskStatus.Created)
    {
        var items = new List<PickingItem>
        {
            new PickingItem(
                Guid.NewGuid(),
                "Test Product",
                "SKU-TEST",
                2,
                "A-01-01",
                "BC-TEST"
            )
        };

        var task = new PickingTask(
            orderId ?? Guid.NewGuid(),
            items,
            "Zone-A",
            pickerId
        );

        if (status == PickingTaskStatus.InProgress)
        {
            task.StartPicking(pickerId);
        }
        else if (status == PickingTaskStatus.Completed)
        {
            task.StartPicking(pickerId);
            task.Complete();
        }

        return task;
    }

    public static StorageUnit CreateStorageUnit(
        Guid? productId = null,
        int quantity = 10,
        int reservedQuantity = 0)
    {
        return new StorageUnit(
            productId ?? Guid.NewGuid(),
            "Test Product",
            "SKU-TEST",
            quantity,
            "A-01-01",
            "Zone-A"
        )
        {
            // Устанавливаем зарезервированное количество через reflection
            // так как свойство private set
        };
    }
}