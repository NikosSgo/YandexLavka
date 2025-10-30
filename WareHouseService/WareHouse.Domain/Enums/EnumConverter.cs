using System.ComponentModel;

namespace WareHouse.Domain.Enums;

public static class EnumConverter
{
    public static string ToString(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Received => "Received",
            OrderStatus.Picking => "Picking",
            OrderStatus.Picked => "Picked",
            OrderStatus.Completed => "Completed",
            OrderStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    public static OrderStatus ToOrderStatus(string status)
    {
        return status.ToLower() switch
        {
            "received" => OrderStatus.Received,
            "picking" => OrderStatus.Picking,
            "picked" => OrderStatus.Picked,
            "completed" => OrderStatus.Completed,
            "cancelled" => OrderStatus.Cancelled,
            _ => throw new ArgumentException($"Unknown order status: {status}")
        };
    }

    public static string ToString(PickingTaskStatus status)
    {
        return status switch
        {
            PickingTaskStatus.Created => "Created",
            PickingTaskStatus.InProgress => "InProgress",
            PickingTaskStatus.Completed => "Completed",
            PickingTaskStatus.Cancelled => "Cancelled",
            _ => status.ToString()
        };
    }

    public static PickingTaskStatus ToPickingTaskStatus(string status)
    {
        return status.ToLower() switch
        {
            "created" => PickingTaskStatus.Created,
            "inprogress" => PickingTaskStatus.InProgress,
            "completed" => PickingTaskStatus.Completed,
            "cancelled" => PickingTaskStatus.Cancelled,
            _ => throw new ArgumentException($"Unknown picking task status: {status}")
        };
    }
}